using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class MethodSpecifyForSendCustomNetworkEventCannotStartWithUnderscoreTest : DiagnosticVerifier<MethodSpecifyForSendCustomNetworkEventCannotStartWithUnderscore>
    {
        [Fact]
        public async Task UdonSharpBehaviourSendCustomNetworkEventMethodNotStartsWithUnderscoreInAnotherClassHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;

using VRC.Udon.Common.Interfaces;

namespace UdonRabbit
{
    public class TestBehaviour1 : UdonSharpBehaviour
    {
        [SerializeField]
        private TestBehaviour2 _behaviour;

        private void Update()
        {
            _behaviour.SendCustomNetworkEvent(NetworkEventTarget.All, ""SomeNetworkEvent"");
        }
    }

    public class TestBehaviour2 : UdonSharpBehaviour
    {
        public void SomeNetworkEvent()
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourSendCustomNetworkEventMethodStartsWithUnderscoreInAnotherClassHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(MethodSpecifyForSendCustomNetworkEventCannotStartWithUnderscore.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Warning)
                             .WithArguments("_SomeNetworkEvent", "UdonRabbit.TestBehaviour2");

            const string source = @"
using UdonSharp;

using UnityEngine;

using VRC.Udon.Common.Interfaces;

namespace UdonRabbit
{
    public class TestBehaviour1 : UdonSharpBehaviour
    {
        [SerializeField]
        private TestBehaviour2 _behaviour;

        private void Update()
        {
            [|_behaviour.SendCustomNetworkEvent|](NetworkEventTarget.All, ""_SomeNetworkEvent"");
        }
    }

    public class TestBehaviour2 : UdonSharpBehaviour
    {
        public void _SomeNetworkEvent()
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}