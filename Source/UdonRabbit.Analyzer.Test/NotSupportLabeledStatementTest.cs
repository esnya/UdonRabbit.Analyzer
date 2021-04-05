using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportLabeledStatementTest : DiagnosticVerifier<NotSupportLabeledStatement>
    {
        [Fact]
        public async Task MonoBehaviourLabeledStatementHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        private void Update()
        {
Label1:
            Debug.Log(""Hello, World"");
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviour__HasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportLabeledStatement.ComponentId)
                             .WithLocation(12, 1)
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
Label1:
            Debug.Log(""Hello, World"");
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}