using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class MethodIsNotExposedToUdonTest : DiagnosticVerifier<MethodIsNotExposedToUdon>
    {
        [Fact]
        public async Task MonoBehaviourInstanceMethodHasNoDiagnosticReport()
        {
            const string source = @"
using UnityEngine;

namespace UdonRabbit
{
    public class TestClass : MonoBehaviour
    {
        public ParticleSystem ps;

        private void Update()
        {
            ps.Play();
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedInstanceMethodHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        public ParticleSystem ps;

        private void Update()
        {
            ps.Play();
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedStaticMethodHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        private void Start()
        {
            var s = ""1"";
            int i;

            var b = int.TryParse(s, out i);
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedVrcMethodHasNoDiagnosticReport()
        {
            const string source = @"
using UdonSharp;

using VRC.SDKBase;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Update()
        {
            if (!Networking.IsOwner(gameObject))
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourNotAllowedInstanceMethodHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(MethodIsNotExposedToUdon.ComponentId)
                             .WithLocation(14, 21)
                             .WithSeverity(DiagnosticSeverity.Error)
                             .WithArguments("GetSafeCollisionEventSize");

            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        public ParticleSystem ps;

        private void Update()
        {
            var i = ps.GetSafeCollisionEventSize();
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}