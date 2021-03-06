using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class MethodSpecifyForSendCustomNetworkEventMustBePublicTest : CodeFixVerifier<MethodSpecifyForSendCustomNetworkEventMustBePublic, MethodSpecifyForSendCustomNetworkEventMustBePublicCodeFixProvider>
    {
        [Fact]
        public async Task UdonSharpBehaviourSendCustomEventDelayedFramesThatAnotherReceiverMethodIsNonPublicHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(MethodSpecifyForSendCustomNetworkEventMustBePublic.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Warning)
                             .WithArguments("SomeNetworkEvent", "UdonRabbit.TestBehaviour2");

            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour1 : UdonSharpBehaviour
    {
        [SerializeField]
        private TestBehaviour2 _behaviour;

        private void Update()
        {
            [|_behaviour.SendCustomEventDelayedFrames|](""SomeNetworkEvent"", 10);
        }
    }

    public class TestBehaviour2 : UdonSharpBehaviour
    {
        private void SomeNetworkEvent()
        {
        }
    }
}
";

            const string newSource = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour1 : UdonSharpBehaviour
    {
        [SerializeField]
        private TestBehaviour2 _behaviour;

        private void Update()
        {
            _behaviour.SendCustomEventDelayedFrames(""SomeNetworkEvent"", 10);
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

            await VerifyCodeFixAsync(source, new[] { diagnostic }, newSource);
        }

        [Fact]
        public async Task UdonSharpBehaviourSendCustomEventDelayedSecondsThatAnotherReceiverMethodIsNonPublicHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(MethodSpecifyForSendCustomNetworkEventMustBePublic.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Warning)
                             .WithArguments("SomeNetworkEvent", "UdonRabbit.TestBehaviour2");

            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour1 : UdonSharpBehaviour
    {
        [SerializeField]
        private TestBehaviour2 _behaviour;

        private void Update()
        {
            [|_behaviour.SendCustomEventDelayedSeconds|](""SomeNetworkEvent"", 10);
        }
    }

    public class TestBehaviour2 : UdonSharpBehaviour
    {
        private void SomeNetworkEvent()
        {
        }
    }
}
";

            const string newSource = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour1 : UdonSharpBehaviour
    {
        [SerializeField]
        private TestBehaviour2 _behaviour;

        private void Update()
        {
            _behaviour.SendCustomEventDelayedSeconds(""SomeNetworkEvent"", 10);
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

            await VerifyCodeFixAsync(source, new[] { diagnostic }, newSource);
        }

        [Fact]
        public async Task UdonSharpBehaviourSendCustomEventThatAnotherReceiverMethodIsNonPublicHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(MethodSpecifyForSendCustomNetworkEventMustBePublic.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Warning)
                             .WithArguments("SomeNetworkEvent", "UdonRabbit.TestBehaviour2");

            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour1 : UdonSharpBehaviour
    {
        [SerializeField]
        private TestBehaviour2 _behaviour;

        private void Update()
        {
            [|_behaviour.SendCustomEvent|](""SomeNetworkEvent"");
        }
    }

    public class TestBehaviour2 : UdonSharpBehaviour
    {
        private void SomeNetworkEvent()
        {
        }
    }
}
";

            const string newSource = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour1 : UdonSharpBehaviour
    {
        [SerializeField]
        private TestBehaviour2 _behaviour;

        private void Update()
        {
            _behaviour.SendCustomEvent(""SomeNetworkEvent"");
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

            await VerifyCodeFixAsync(source, new[] { diagnostic }, newSource);
        }

        [Fact]
        public async Task UdonSharpBehaviourSendCustomEventThatAnotherReceiverMethodIsPublicHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour1 : UdonSharpBehaviour
    {
        [SerializeField]
        private TestBehaviour2 _behaviour;

        private void Update()
        {
            _behaviour.SendCustomEvent(""SomeNetworkEvent"");
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
        public async Task UdonSharpBehaviourSendCustomNetworkEventThatAnotherReceiverMethodIsNonPublicHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(MethodSpecifyForSendCustomNetworkEventMustBePublic.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Warning)
                             .WithArguments("SomeNetworkEvent", "UdonRabbit.TestBehaviour2");

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
            [|_behaviour.SendCustomNetworkEvent|](NetworkEventTarget.All, ""SomeNetworkEvent"");
        }
    }

    public class TestBehaviour2 : UdonSharpBehaviour
    {
        private void SomeNetworkEvent()
        {
        }
    }
}
";

            const string newSource = @"
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

            await VerifyCodeFixAsync(source, new[] { diagnostic }, newSource);
        }
    }
}