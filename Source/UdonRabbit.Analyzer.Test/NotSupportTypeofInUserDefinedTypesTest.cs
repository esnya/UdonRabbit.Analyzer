using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportTypeofInUserDefinedTypesTest : DiagnosticVerifier<NotSupportTypeofInUserDefinedTypes>
    {
        [Fact]
        public async Task MonoBehaviourTypeofInUserDefinedTypeHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        private void Start()
        {
            var t = typeof(TestBehaviour);
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourTypeofInUnityDefinedTypeHasNoDiagnosticsReport()
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
            var t = typeof(Transform);
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourTypeofInUserDefinedArrayTypeHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            var t = typeof(TestBehaviour[]);
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourTypeofInUserDefinedTypeHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportTypeofInUserDefinedTypes.ComponentId)
                             .WithLocation(10, 21)
                             .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            var t = typeof(TestBehaviour);
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}