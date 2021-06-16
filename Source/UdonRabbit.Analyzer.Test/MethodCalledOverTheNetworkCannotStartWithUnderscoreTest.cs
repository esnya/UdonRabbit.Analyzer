using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class MethodCalledOverTheNetworkCannotStartWithUnderscoreTest : CodeFixVerifier<MethodCalledOverTheNetworkCannotStartWithUnderscore, MethodCalledOverTheNetworkCannotStartWithUnderscoreCodeFixProvider>
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
        public async Task UdonSharpBehaviourSendCustomNetworkEventMethodStartsWithUnderscoreByNameofOperatorHasDiagnosticsReport()
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

            const string newSource = @"
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

            await VerifyCodeFixAsync(source, new[] { diagnostic }, newSource);
        }

        [Fact]
        public async Task UdonSharpBehaviourSendCustomNetworkEventMethodStartsWithUnderscoreByStringLiteralHasDiagnosticsReport()
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
            SendCustomNetworkEvent(NetworkEventTarget.All, ""_SomeNetworkEvent"");
        }

        [|public void _SomeNetworkEvent()
        {
        }|]
    }
}
";

            const string newSource = @"
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

        public void SomeNetworkEvent()
        {
        }
    }
}
";

            await VerifyCodeFixAsync(source, new[] { diagnostic }, newSource);
        }
    }
}