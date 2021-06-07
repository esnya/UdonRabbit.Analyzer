using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportObjectInitializerTest : DiagnosticVerifier<NotSupportObjectInitializer>
    {
        [Fact]
        public async Task MonoBehaviourObjectInitializerHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        private void Start()
        {
            var obj = new GameObject { tag = ""some tag"" };
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourObjectCreationHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            var obj = new GameObject();
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourObjectInitializerHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportObjectInitializer.ComponentId)
                .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            var obj = new GameObject [|{ tag = ""some tag"" }|];
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}