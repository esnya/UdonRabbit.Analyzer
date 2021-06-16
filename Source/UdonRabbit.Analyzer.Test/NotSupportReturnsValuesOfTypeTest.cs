using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportReturnsValuesOfTypeTest : DiagnosticVerifier<NotSupportReturnsValuesOfType>
    {
        [Fact]
        public async Task MonoBehaviourUdonNotAllowedReturnValueInMethodDeclarationHasNoDiagnosticsReports()
        {
            const string source = @"
using System;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        private IntPtr GetTypeOf()
        {
            return IntPtr.Zero;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourArrayOfUdonSharpBehaviourReturnValueInMethodDeclarationHasNoDiagnosticsReports()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [SerializeField]
        private UdonSharpBehaviour[] _behaviour;

        private UdonSharpBehaviour[] GetTypeOf()
        {
            return _behaviour;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourNoReturnValueInMethodDeclarationHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void SomeMethod()
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourReturnArrayTypeInheritFromUdonSharpBehaviourHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private TestBehaviour[] SomeMethod()
        {
            return new TestBehaviour[0];
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourReturnTypeInheritFromUdonSharpBehaviourHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private TestBehaviour SomeMethod()
        {
            return new TestBehaviour();
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonAllowedPrimitiveReturnValueInMethodDeclarationHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private int SomeMethod()
        {
            return 1;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonAllowedReturnValueInInnerTypeInMethodDeclarationHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using VRC.SDKBase;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private VRCPlayerApi.TrackingDataType _trackingDataType;

        private VRCPlayerApi.TrackingDataType GetTrackingType()
        {
            return _trackingDataType;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonAllowedReturnValueInMethodDeclarationHasNoDiagnosticsReports()
        {
            const string source = @"
using System;

using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private Type GetTypeOf()
        {
            return typeof(void);
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonBehaviourReturnValueInMethodDeclarationHasNoDiagnosticsReports()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;

using VRC.Udon;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [SerializeField]
        private UdonBehaviour _behaviour;

        private UdonBehaviour GetTypeOf()
        {
            return _behaviour;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonNotAllowedJaggedArrayReturnValueInMethodDeclarationHasNoDiagnosticsReports()
        {
            var diagnostic = ExpectDiagnostic(NotSupportReturnsValuesOfType.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Error)
                             .WithArguments("System.IntPtr[][]");

            const string source = @"
using System;

using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [|private IntPtr[][] GetTypeOf()
        {
            return new IntPtr[1][];
        }|]
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonNotAllowedReturnValueInMethodDeclarationHasNoDiagnosticsReports()
        {
            var diagnostic = ExpectDiagnostic(NotSupportReturnsValuesOfType.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Error)
                             .WithArguments("System.IntPtr");

            const string source = @"
using System;

using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [|private IntPtr GetTypeOf()
        {
            return IntPtr.Zero;
        }|]
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonSharpBehaviourReturnValueInMethodDeclarationHasNoDiagnosticsReports()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [SerializeField]
        private UdonSharpBehaviour _behaviour;

        private UdonSharpBehaviour GetTypeOf()
        {
            return _behaviour;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }
    }
}