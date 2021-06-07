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
}";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedConstructorHasNoDiagnosticsReport()
        {
            const string source = @"
using System.Diagnostics;

using UdonSharp;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        private void Update()
        {
            var i = new Stopwatch();
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedConstructorWithParametersHasNoDiagnosticsReport()
        {
            const string source = @"
using System;

using UdonSharp;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        private void Update()
        {
            var i = new DateTimeOffset(2021, 4, 6, 17, 29, 0, new TimeSpan(9, 0, 0));
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedEnumToStringHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using VRC.SDK3.Components.Video;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        private void Update()
        {
            var s = VideoError.Unknown.ToString();
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedEnumToStringViaFieldHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using VRC.SDK3.Components.Video;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        public override void OnVideoError(VideoError err)
        {
            var s = err.ToString();
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
        public async Task UdonSharpBehaviourNotAllowedConstructorHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(MethodIsNotExposedToUdon.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Error)
                             .WithArguments(".ctor");

            const string source = @"
using UdonSharp;

using TMPro;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        private void Update()
        {
            var i = [|new TextMeshProUGUI()|];
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourNotAllowedInstanceMethodHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(MethodIsNotExposedToUdon.ComponentId)
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
            var i = [|ps.GetSafeCollisionEventSize()|];
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}