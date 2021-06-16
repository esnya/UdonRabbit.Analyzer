using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportVariableTweeningWhenBehaviourIsManualSyncModeTest : DiagnosticVerifier<NotSupportVariableTweeningWhenBehaviourIsManualSyncMode>
    {
        [Fact]
        public async Task MonoBehaviourNoSupportTweeningPatternHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class TestBehaviour : MonoBehaviour
    {
        [UdonSynced(UdonSyncMode.Linear)]
        private int _data;
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourNoSupportTweeningPatternHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportVariableTweeningWhenBehaviourIsManualSyncMode.ComponentId)
                .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class TestBehaviour : UdonSharpBehaviour
    {
        [|[UdonSynced(UdonSyncMode.Linear)]
        private int _data;|]
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourSupportTweeningPatternHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class TestBehaviour : MonoBehaviour
    {
        [UdonSynced(UdonSyncMode.Linear)]
        private int _data;
    }
}
";

            await VerifyAnalyzerAsync(source);
        }
    }
}