using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportIsKeywordInExpressionTest : DiagnosticVerifier<NotSupportIsKeywordInExpression>
    {
        [Fact]
        public async Task MonoBehaviourIsExpressionHasNoDiagnosticsReport()
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
            if (_component is Transform)
            {
                var t = (Transform) _component;
            }
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task MonoBehaviourIsPatternExpressionHasNoDiagnosticsReport()
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
            if (_component is Transform t)
            {
                var position = t.position;
            }
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourIsExpressionHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportIsKeywordInExpression.ComponentId)
                             .WithLocation(15, 17)
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
            if (_component is Transform)
            {
                var t = (Transform) _component;
            }
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourIsPatternExpressionHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportIsKeywordInExpression.ComponentId)
                             .WithLocation(15, 17)
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
            if (_component is Transform t)
            {
                var position = t.position;
            }
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}