using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportDefaultExpressionsTest : DiagnosticVerifier<NotSupportDefaultExpressions>
    {
        [Fact]
        public async Task MonoBehaviourClassDefaultExpressionHasNotDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        private int _initValue = default(int);
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourClassDefaultExpressionHasNotDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportDefaultExpressions.ComponentId)
                             .WithLocation(8, 34)
                             .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private int _initValue = default(int);
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}