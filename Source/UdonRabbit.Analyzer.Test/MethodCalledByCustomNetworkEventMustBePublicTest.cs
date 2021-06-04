using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class MethodCalledByCustomNetworkEventMustBePublicTest : DiagnosticVerifier<MethodCalledByCustomNetworkEventMustBePublic>
    {
        [Fact]
        public async Task UdonSharpBehaviourSendCustomNetworkEventByNameOfFullSignatureIsNonPublicHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(MethodCalledByCustomNetworkEventMustBePublic.ComponentId)
                             .WithLocation(15, 9)
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
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(TestBehaviour.SomeNetworkEvent));
        }

        private void SomeNetworkEvent()
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourSendCustomNetworkEventByNameOfSignatureIsNonPublicHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(MethodCalledByCustomNetworkEventMustBePublic.ComponentId)
                             .WithLocation(15, 9)
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
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(SomeNetworkEvent));
        }

        private void SomeNetworkEvent()
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourSendCustomNetworkEventIsNonPublicHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(MethodCalledByCustomNetworkEventMustBePublic.ComponentId)
                             .WithLocation(15, 9)
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
            SendCustomNetworkEvent(NetworkEventTarget.All, ""SomeNetworkEvent"");
        }

        private void SomeNetworkEvent()
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourSendMethodCustomEventIsNonPublicHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(MethodCalledByCustomNetworkEventMustBePublic.ComponentId)
                             .WithLocation(13, 9)
                             .WithSeverity(DiagnosticSeverity.Warning);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Update()
        {
            SendCustomEvent(""SomeNetworkEvent"");
        }

        private void SomeNetworkEvent()
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourSendMethodCustomEventIsPublicHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Update()
        {
            SendCustomEvent(""SomeNetworkEvent"");
        }

        public void SomeNetworkEvent()
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }
    }
}