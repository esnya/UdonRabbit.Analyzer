using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportLinearInterpolationOfSyncedTypeTest : DiagnosticVerifier<NotSupportLinearInterpolationOfSyncedType>
    {
        [Fact]
        public async Task UdonSharpBehaviourNotSupportLinearInterpolationSyncTypeHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportLinearInterpolationOfSyncedType.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Error)
                             .WithArguments("bool");

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [|[UdonSynced(UdonSyncMode.Linear)]
        private bool _b;|]
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourSupportLinearInterpolationSyncTypeHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [UdonSynced(UdonSyncMode.Linear)]
        private int _b;
    }
}
";

            await VerifyAnalyzerAsync(source);
        }
    }
}