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
        public async Task UdonSharpBehaviourUdonNotAllowedReturnValueInMethodDeclarationHasNoDiagnosticsReports()
        {
            var diagnostic = ExpectDiagnostic(NotSupportReturnsValuesOfType.ComponentId)
                             .WithLocation(10, 9)
                             .WithSeverity(DiagnosticSeverity.Error)
                             .WithArguments("System.IntPtr");

            const string source = @"
using System;

using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private IntPtr GetTypeOf()
        {
            return IntPtr.Zero;
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}