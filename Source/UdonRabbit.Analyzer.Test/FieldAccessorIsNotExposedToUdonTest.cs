using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class FieldAccessorIsNotExposedToUdonTest : DiagnosticVerifier<FieldAccessorIsNotExposedToUdon>
    {
        [Fact]
        public async Task UdonSharpBehaviourAllowedEnumField1HasNoDiagnosticsReport()
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
            var s = VideoError.Unknown;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedEnumField2HasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using VRC.Udon.Common.Interfaces;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        private void Update()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, ""Emit"");
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedFieldAccessorHasNoDiagnosticsReport()
        {
            const string source = @"
using TMPro;

using UdonSharp;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        public TextMeshProUGUI tm;

        private void Update()
        {
            var t = tm.text;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedFieldAccessorInInnerClassHasNoDiagnosticsReport()
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
        public async Task UdonSharpBehaviourAllowedGameObjectAccessorHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;
using UnityEngine.Animations;

using VRC.Udon;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        [SerializeField]
        private UdonSharpBehaviour _behaviour1; // allowed by whitelist

        [SerializeField]
        private UdonBehaviour _behaviour2; // allowed by workaround

        [SerializeField]
        private ScaleConstraint _constraint; // allowed by Udon

        private void Start()
        {
            var go1 = _behaviour1.gameObject;
            var go2 = _behaviour2.gameObject;
            var go3 = _constraint.gameObject;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedInstancePropertyOfJaggedArraysHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private int[][] _array;

        private void Start()
        {
            _array = new int[4][];
            var i = _array.Length;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedInstancePropertyOfJaggedArraysOfUdonBehaviourHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using VRC.Udon;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private UdonBehaviour[][] _array;

        private void Start()
        {
            _array = new UdonBehaviour[4][];
            var i = _array.Length;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedInstancePropertyOfJaggedArraysOfUdonSharpBehaviourHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private UdonSharpBehaviour[][] _array;

        private void Start()
        {
            _array = new UdonSharpBehaviour[4][];
            var i = _array.Length;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedPrimitivePropertyAccessorHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private readonly string str = ""Hello, World"";

        private void Start()
        {
            var len = str.Length;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedStaticFieldAccessorHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        private void Update()
        {
            var v = Vector3.one;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedStaticFieldAccessorInIfStatementHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        private void Update()
        {
            if (Vector3.one == Vector3.zero) { }
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedStaticFieldAccessorIntoLocalVariableHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        private Vector3 _initialScale;

        private void Update()
        {
            _initialScale = Vector3.one;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedVrcInstanceFieldHasNoDiagnosticsReport()
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

        private void Start()
        {
            var text = _pickup.InteractionText;
            _pickup.InteractionText = text;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedVrcStaticFieldAccessorHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using VRC.SDKBase;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            var player = Networking.LocalPlayer;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourNotAllowedFieldAccessorHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(FieldAccessorIsNotExposedToUdon.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Error)
                             .WithArguments("font");

            const string source = @"
using TMPro;

using UdonSharp;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        public TextMeshProUGUI tm;

        private void Update()
        {
            var f = [|tm.font|];
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourNotAllowedGameObjectAccessorHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(FieldAccessorIsNotExposedToUdon.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Error)
                             .WithArguments("gameObject");

            const string source = @"
using UdonSharp;

using VRC.SDKBase;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        private void Start()
        {
            var go = [|Networking.LocalPlayer.gameObject|];
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourNotAllowedUnityGameObjectAccessorHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(FieldAccessorIsNotExposedToUdon.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Error)
                             .WithArguments("gameObject");

            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        [SerializeField]
        private ParticleSystemForceField _field;

        private void Start()
        {
            var go = [|_field.gameObject|];
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}