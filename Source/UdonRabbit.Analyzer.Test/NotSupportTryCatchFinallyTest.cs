using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportTryCatchFinallyTest : DiagnosticVerifier<NotSupportTryCatchFinally>
    {
        [Fact]
        public async Task MonoBehaviourClassTryCatchStatementHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

namespace UdonRabbit
{
    public class TestClass : MonoBehaviour
    {
        private void Update()
        {
            object o = null;

            try
            {
                var i = (int) o;
            }
            catch
            {
            }
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourClassTryCatchStatementHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportTryCatchFinally.ComponentId)
                             .WithLocation(12, 13)
                             .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        private void Update()
        {
            object o = null;

            try
            {
                var i = (int) o;
            }
            catch
            {
            }
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourClassTryFinallyStatementHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportTryCatchFinally.ComponentId)
                             .WithLocation(12, 13)
                             .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        private void Update()
        {
            object o = null;

            try
            {
                var i = (int) o;
            }
            finally
            {
            }
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}