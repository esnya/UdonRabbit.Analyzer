using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportNullConditionalOperatorsTest : DiagnosticVerifier<NotSupportNullConditionalOperators>
    {
        [Fact]
        public async Task MonoBehaviourNullConditionalOperatorHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        [SerializeField]
        private GameObject _go;

        private void Start()
        {
            var t = _go?.GetComponent<Transform>();
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourNullConditionalOperatorHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportNullConditionalOperators.ComponentId)
                .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [SerializeField]
        private GameObject _go;

        private void Start()
        {
            var t = [|_go?.GetComponent<Transform>()|];
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}