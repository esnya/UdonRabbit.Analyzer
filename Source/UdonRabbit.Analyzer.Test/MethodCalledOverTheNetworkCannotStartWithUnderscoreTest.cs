using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class MethodCalledOverTheNetworkCannotStartWithUnderscoreTest : DiagnosticVerifier<MethodCalledOverTheNetworkCannotStartWithUnderscore>
    {
        [Fact]
        public async Task UdonSharpBehaviourSendCustomNetworkEventMethodNotStartsWithUnderscoreHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using VRC.Udon.Common.Interfaces;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Update()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(TestBehaviour.SomeNetworkEvent));
        }

        public void SomeNetworkEvent()
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourSendCustomNetworkEventMethodStartsWithUnderscoreHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(MethodCalledOverTheNetworkCannotStartWithUnderscore.ComponentId)
                .WithSeverity(DiagnosticSeverity.Warning);

            const string source = @"
using UdonSharp;

using VRC.Udon.Common.Interfaces;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Update()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(TestBehaviour._SomeNetworkEvent));
        }

        [|public void _SomeNetworkEvent()
        {
        }|]
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}