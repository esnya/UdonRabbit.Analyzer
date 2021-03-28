using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportBaseTypeCallingTest : DiagnosticVerifier<NotSupportBaseTypeCalling>
    {
        [Fact]
        public async Task MonoBehaviourClassBaseTypeCallingHasNotDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        private void Update()
        {
            base.GetComponent<Transform>();
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourClassBaseTypeCallingHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportBaseTypeCalling.ComponentId)
                             .WithLocation(12, 13)
                             .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Update()
        {
            base.GetComponent<Transform>();
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}