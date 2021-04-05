using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class OnlySupportSyncingArrayTypeInManualSyncModeTest : DiagnosticVerifier<OnlySupportSyncingArrayTypeInManualSyncMode>
    {
        public OnlySupportSyncingArrayTypeInManualSyncModeTest()
        {
            // Udon Networking Beta has validator of linear interpolation sync type, but other SDKs not worked correctly
            _hasSupportUdonNetworkingTypes = false;
        }

        private readonly bool _hasSupportUdonNetworkingTypes;

        [Fact]
        public async Task MonoBehaviourSyncingArrayTypeInContinuousSyncModeHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class TestBehaviour : MonoBehaviour
    {
        [UdonSynced]
        private int[] _data;
    }
}
";

            if (_hasSupportUdonNetworkingTypes)
                await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourSyncingArrayTypeInContinuousSyncModeHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(OnlySupportSyncingArrayTypeInManualSyncMode.ComponentId)
                             .WithLocation(9, 9)
                             .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class TestBehaviour : UdonSharpBehaviour
    {
        [UdonSynced]
        private int[] _data;
    }
}
";

            if (_hasSupportUdonNetworkingTypes)
                await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourSyncingArrayTypeInManualSycModeHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class TestBehaviour : UdonSharpBehaviour
    {
        [UdonSynced]
        private int[] _data;
    }
}
";

            if (_hasSupportUdonNetworkingTypes)
                await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourSyncingNotArrayTypeInContinuousSycModeHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class TestBehaviour : UdonSharpBehaviour
    {
        [UdonSynced]
        private int _data;
    }
}
";

            if (_hasSupportUdonNetworkingTypes)
                await VerifyAnalyzerAsync(source);
        }
    }
}