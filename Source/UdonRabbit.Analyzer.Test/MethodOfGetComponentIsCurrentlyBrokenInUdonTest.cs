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

            const string source = @"
using UdonSharp;

using VRC.SDK3.Components;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            var component1 = [|GetComponent<VRCPickup>()|];
            var component2 = [|GetComponentsInChildren<VRCPickup>(true)|];
        }
    }
}
";

            const string newSource = @"
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

            await VerifyCodeFixAsync(source, new[] { diagnostic1, diagnostic2 }, newSource);
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