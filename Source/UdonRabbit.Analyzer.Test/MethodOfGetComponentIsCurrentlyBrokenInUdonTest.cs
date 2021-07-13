using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class MethodOfGetComponentIsCurrentlyBrokenInUdonTest : CodeFixVerifier<MethodOfGetComponentIsCurrentlyBrokenInUdon, MethodOfGetComponentIsCurrentlyBrokenInUdonCodeFixProvider>
    {
        [Fact]
        public async Task UdonSharpBehaviourGetComponentDoesNotHaveGenericParameterHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using VRC.SDK3.Components;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            var component1 = GetComponent(typeof(VRCPickup));
            var component2 = GetComponentsInChildren(typeof(VRCPickup), true);
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourGetComponentHasGenericParameterAndSpecifyBrokenComponentsHasDiagnosticsReport()
        {
            var diagnostic1 = ExpectDiagnostic(MethodOfGetComponentIsCurrentlyBrokenInUdon.ComponentId)
                              .WithSeverity(DiagnosticSeverity.Error)
                              .WithArguments("GetComponent");

            var diagnostic2 = ExpectDiagnostic(MethodOfGetComponentIsCurrentlyBrokenInUdon.ComponentId)
                              .WithSeverity(DiagnosticSeverity.Error)
                              .WithArguments("GetComponentsInChildren");

            var diagnostic3 = ExpectDiagnostic(MethodOfGetComponentIsCurrentlyBrokenInUdon.ComponentId)
                              .WithSeverity(DiagnosticSeverity.Error)
                              .WithArguments("GetComponent");

            var diagnostic4 = ExpectDiagnostic(MethodOfGetComponentIsCurrentlyBrokenInUdon.ComponentId)
                              .WithSeverity(DiagnosticSeverity.Error)
                              .WithArguments("GetComponent");

            const string source = @"
using UdonSharp;

using UnityEngine;

using VRC.SDK3.Components;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [SerializeField]
        private TestBehaviour _behaviour;

        public TestBehaviour InnerBehaviour => _behaviour;

        private void Start()
        {
            var component1 = [|GetComponent<VRCPickup>()|];
            var component2 = [|GetComponentsInChildren<VRCPickup>(true)|];
            var component3 = [|_behaviour.GetComponent<VRCPickup>()|];
            var component4 = [|_behaviour.InnerBehaviour.GetComponent<VRCPickup>()|];
        }
    }
}
";

            const string newSource = @"
using UdonSharp;

using UnityEngine;

using VRC.SDK3.Components;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [SerializeField]
        private TestBehaviour _behaviour;

        public TestBehaviour InnerBehaviour => _behaviour;

        private void Start()
        {
            var component1 = (VRCPickup)GetComponent(typeof(VRCPickup));
            var component2 = (VRCPickup)GetComponentsInChildren(typeof(VRCPickup), true);
            var component3 = (VRCPickup)_behaviour.GetComponent(typeof(VRCPickup));
            var component4 = (VRCPickup)_behaviour.InnerBehaviour.GetComponent(typeof(VRCPickup));
        }
    }
}
";

            await VerifyCodeFixAsync(source, new[] { diagnostic1, diagnostic2, diagnostic3, diagnostic4 }, newSource);
        }

        [Fact]
        public async Task UdonSharpBehaviourGetComponentHasGenericParameterAndSpecifyNotBrokenComponentsHasDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            var component1 = GetComponent(typeof(Transform));
            var component2 = GetComponentsInChildren(typeof(Transform), true);
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }
    }
}