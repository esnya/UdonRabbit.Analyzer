using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportAsKeywordInExpressionTest : DiagnosticVerifier<NotSupportAsKeywordInExpression>
    {
        [Fact]
        public async Task MonoBehaviourAsExpressionHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        [SerializeField]
        private Component _component;

        private void Start()
        {
            var t = _component as Transform;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAsExpressionHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportAsKeywordInExpression.ComponentId)
                .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [SerializeField]
        private Component _component;

        private void Start()
        {
            var t = [|_component as Transform|];
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}