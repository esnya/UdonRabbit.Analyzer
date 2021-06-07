using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportStaticUsingDirectiveTest : DiagnosticVerifier<NotSupportStaticUsingDirective>
    {
        [Fact]
        public async Task MonoBehaviourStaticUsingDirectiveHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

using static UnityEngine.Debug;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        private void Start()
        {
            Log(""Hello"");
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourStaticUsingDirectiveHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportStaticUsingDirective.ComponentId)
                .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

[|using static UnityEngine.Debug;|]

//
namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            Log(""Hello"");
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}