using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class FieldAccessorIsNotExposedToUdonTest : DiagnosticVerifier<FieldAccessorIsNotExposedToUdon>
    {
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
        public async Task UdonSharpBehaviourAllowedFieldAccessorIsNoDiagnosticsReport()
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
        public async Task UdonSharpBehaviourAllowedStaticFieldAccessorIntoLocalVariableIsNoDiagnosticsReport()
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
        public async Task UdonSharpBehaviourAllowedStaticFieldAccessorIsNoDiagnosticsReport()
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
        public async Task UdonSharpBehaviourAllowedVrcFieldAccessorHasNoDiagnosticsReport()
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
                             .WithLocation(14, 21)
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
            var f = tm.font;
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}