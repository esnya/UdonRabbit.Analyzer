using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportVariableTweeningWhenBehaviourIsManualSyncModeTest : DiagnosticVerifier<NotSupportVariableTweeningWhenBehaviourIsManualSyncMode>
    {
        public NotSupportVariableTweeningWhenBehaviourIsManualSyncModeTest()
        {
            // Udon Networking Beta has validator of linear interpolation sync type, but other SDKs not worked correctly
            _hasSupportUdonNetworkingTypes = false;
        }

        private readonly bool _hasSupportUdonNetworkingTypes;

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

            if (_hasSupportUdonNetworkingTypes)
                await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourNoSupportTweeningPatternHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportVariableTweeningWhenBehaviourIsManualSyncMode.ComponentId)
                             .WithLocation(9, 9)
                             .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

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

            if (_hasSupportUdonNetworkingTypes)
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
    [UdonBehaviourSyncMode]
    public class TestBehaviour : MonoBehaviour
    {
        [UdonSynced(UdonSyncMode.Linear)]
        private int _data;
    }
}
";

            if (_hasSupportUdonNetworkingTypes)
                await VerifyAnalyzerAsync(source);
        }
    }
}