using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class MethodSpecifyForSendCustomEventIsNotFoundInBehaviourTest : CodeFixVerifier<MethodSpecifyForSendCustomEventIsNotFoundInBehaviour, MethodSpecifyForSendCustomEventIsNotFoundInBehaviourCodeFixProvider>
    {
        [Fact]
        public async Task UdonSharpBehaviourMethodSpecifyForSendCustomEventDelayedFramesIsDeclaredInSameClassHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Update()
        {
            SendCustomEventDelayedFrames(""SomeMethod"", 10);
        }

        public void SomeMethod()
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourMethodSpecifyForSendCustomEventDelayedFramesIsNotDeclaredInSameClassHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(MethodSpecifyForSendCustomEventIsNotFoundInBehaviour.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Warning)
                             .WithArguments("SomeMethod", "SendCustomEventDelayedFrames", "TestBehaviour");

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Update()
        {
            [|SendCustomEventDelayedFrames(""SomeMethod"", 10)|];
        }
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
            SendCustomEventDelayedFrames(""SomeMethod"", 10);
        }

        public void SomeMethod()
        {
        }
    }
}
";

            await VerifyCodeFixAsync(source, new[] { diagnostic }, newSource);
        }

        [Fact]
        public async Task UdonSharpBehaviourMethodSpecifyForSendCustomEventDelayedSecondsIsNotDeclaredInSameClassHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(MethodSpecifyForSendCustomEventIsNotFoundInBehaviour.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Warning)
                             .WithArguments("SomeMethod", "SendCustomEventDelayedSeconds", "TestBehaviour");

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Update()
        {
            [|SendCustomEventDelayedSeconds(""SomeMethod"", 10)|];
        }
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
            SendCustomEventDelayedSeconds(""SomeMethod"", 10);
        }

        public void SomeMethod()
        {
        }
    }
}
";

            await VerifyCodeFixAsync(source, new[] { diagnostic }, newSource);
        }

        [Fact]
        public async Task UdonSharpBehaviourMethodSpecifyForSendCustomEventIsNotDeclaredInSameClassHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(MethodSpecifyForSendCustomEventIsNotFoundInBehaviour.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Warning)
                             .WithArguments("SomeMethod", "SendCustomEvent", "TestBehaviour");

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Update()
        {
            [|SendCustomEvent(""SomeMethod"")|];
        }
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
            SendCustomEvent(""SomeMethod"");
        }

        public void SomeMethod()
        {
        }
    }
}
";

            await VerifyCodeFixAsync(source, new[] { diagnostic }, newSource);
        }

        [Fact]
        public async Task UdonSharpBehaviourMethodSpecifyForSendCustomNetworkEventIsDeclaredInReceiverClassHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;

using VRC.Udon.Common.Interfaces;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [SerializeField]
        private AnotherBehaviour _behaviour;

        private void Update()
        {
            _behaviour.SendCustomNetworkEvent(NetworkEventTarget.All, ""SomeMethod"");
        }
    }

    public class AnotherBehaviour : UdonSharpBehaviour
    {
        public void SomeMethod()
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourMethodSpecifyForSendCustomNetworkEventIsNotDeclaredInReceiverClassHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(MethodSpecifyForSendCustomEventIsNotFoundInBehaviour.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Warning)
                             .WithArguments("SomeMethod", "SendCustomNetworkEvent", "AnotherBehaviour");

            const string source = @"
using UdonSharp;

using UnityEngine;

using VRC.Udon.Common.Interfaces;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [SerializeField]
        private AnotherBehaviour _behaviour;

        private void Update()
        {
            [|_behaviour.SendCustomNetworkEvent(NetworkEventTarget.All, ""SomeMethod"")|];
        }
    }

    public class AnotherBehaviour : UdonSharpBehaviour
    {
    }
}
";

            const string newSource = @"
using UdonSharp;

using UnityEngine;

using VRC.Udon.Common.Interfaces;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [SerializeField]
        private AnotherBehaviour _behaviour;

        private void Update()
        {
            _behaviour.SendCustomNetworkEvent(NetworkEventTarget.All, ""SomeMethod"");
        }
    }

    public class AnotherBehaviour : UdonSharpBehaviour
    {
        public void SomeMethod()
        {
        }
    }
}
";

            await VerifyCodeFixAsync(source, new[] { diagnostic }, newSource);
        }

        [Fact]
        public async Task UdonSharpBehaviourMethodSpecifyForSendCustomNetworkEventIsNotDeclaredInSameClassHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(MethodSpecifyForSendCustomEventIsNotFoundInBehaviour.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Warning)
                             .WithArguments("SomeMethod", "SendCustomNetworkEvent", "TestBehaviour");

            const string source = @"
using UdonSharp;

using VRC.Udon.Common.Interfaces;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Update()
        {
            [|this.SendCustomNetworkEvent(NetworkEventTarget.All, ""SomeMethod"")|];
        }
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
            this.SendCustomNetworkEvent(NetworkEventTarget.All, ""SomeMethod"");
        }

        public void SomeMethod()
        {
        }
    }
}
";

            await VerifyCodeFixAsync(source, new[] { diagnostic }, newSource);
        }
    }
}