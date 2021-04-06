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
        public async Task UdonSharpBehaviourAllowedInstanceMethodReturnsArrayTHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        [SerializeField]
        private Transform _transform;

        private void Start()
        {
            var components = _transform.GetComponentsInChildren<Transform>();
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedMethodInInnerClassHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using VRC.SDKBase;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        private VRCPlayerApi.TrackingDataType _trackingTarget;
        private VRCPlayerApi _player;

        private void Update()
        {
            var trackingData = _player.GetTrackingData(_trackingTarget);
            transform.SetPositionAndRotation(trackingData.position, trackingData.rotation);
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
        public async Task UdonSharpBehaviourAllowedUdonInstanceMethodHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;

using VRC.Udon;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        [SerializeField]
        private UdonBehaviour _behaviour;

        private void Update()
        {
            _behaviour.SetProgramVariable(""variable"", 1);
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedUnityMethodHasNoDiagnosticReport()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [SerializeField]
        private GameObject _go;

        private void Update()
        {
            var t1 = GetComponent<Transform>();
            var t2 = _go.GetComponent<Transform>();
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedVrcInstanceMethodHasNoDiagnosticReport()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;

using VRC.SDKBase;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [SerializeField]
        private VRC_Pickup _pickup;

        private void Update()
        {
            _pickup.Drop();
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedVrcStaticMethodHasNoDiagnosticReport()
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