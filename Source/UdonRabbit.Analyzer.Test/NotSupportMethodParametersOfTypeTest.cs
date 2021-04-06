using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportMethodParametersOfTypeTest : DiagnosticVerifier<NotSupportMethodParametersOfType>
    {
        [Fact]
        public async Task MonoBehaviourUdonNotAllowedMethodParameterTypeHasNoDiagnosticsReport()
        {
            const string source = @"
using System;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        private void TestMethod(IntPtr ptr)
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourMethodParameterArrayTypeInheritFromUdonSharpBehaviourHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void TestMethod(TestBehaviour[] t)
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourMethodParameterTypeInheritFromUdonSharpBehaviourHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void TestMethod(TestBehaviour t)
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonAllowedMethodParameterPrimitiveTypeHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void TestMethod(int i)
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonAllowedMethodParameterTypeHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void TestMethod(Transform t)
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonBehaviourMethodParameterTypeHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using VRC.Udon;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void TestMethod(UdonBehaviour t)
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonNotAllowedMethodParameterTypeHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportMethodParametersOfType.ComponentId)
                             .WithLocation(10, 33)
                             .WithSeverity(DiagnosticSeverity.Error)
                             .WithArguments("System.IntPtr");

            const string source = @"
using System;

using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void TestMethod(IntPtr ptr)
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonSharpBehaviourMethodParameterTypeHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void TestMethod(UdonSharpBehaviour t)
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }
    }
}