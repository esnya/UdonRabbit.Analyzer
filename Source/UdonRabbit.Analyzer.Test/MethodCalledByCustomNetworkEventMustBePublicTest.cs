using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class MethodCalledByCustomNetworkEventMustBePublicTest : CodeFixVerifier<MethodCalledByCustomNetworkEventMustBePublic, MethodCalledByCustomNetworkEventMustBePublicCodeFixProvider>
    {
        [Fact]
        public async Task UdonSharpBehaviourSendCustomEventMethodByVariableIsPrivateHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Update()
        {
            var arg = ""SomeNetworkEvent"";
            SendCustomEvent(arg);
        }

        private void SomeNetworkEvent()
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourSendCustomEventMethodIsNonPublicHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(MethodCalledByCustomNetworkEventMustBePublic.ComponentId)
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

        [|private void SomeNetworkEvent()
        {
        }|]
    }
}
";

            const string newSource = @"
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

            await VerifyCodeFixAsync(source, new[] { diagnostic }, newSource);
        }

        [Fact]
        public async Task UdonSharpBehaviourSendCustomEventMethodIsPublicHasNoDiagnosticsReport()
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

        [Fact]
        public async Task UdonSharpBehaviourSendCustomNetworkEventMethodByNameOfFullSignatureIsNonPublicHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(MethodCalledByCustomNetworkEventMustBePublic.ComponentId)
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

        [|private void SomeNetworkEvent()
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
        public async Task UdonSharpBehaviourSendCustomNetworkEventMethodByNameOfSignatureIsNonPublicHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(MethodCalledByCustomNetworkEventMustBePublic.ComponentId)
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

        [|private void SomeNetworkEvent()
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
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(SomeNetworkEvent));
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
        public async Task UdonSharpBehaviourSendCustomNetworkEventMethodIsNonPublicHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(MethodCalledByCustomNetworkEventMustBePublic.ComponentId)
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

        [|private void SomeNetworkEvent()
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