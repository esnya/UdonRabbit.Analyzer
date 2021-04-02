using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportVariablesOfTypeTest : DiagnosticVerifier<NotSupportVariablesOfType>
    {
        [Fact]
        public async Task MonoBehaviourUdonNotExposedVariableHasNoDiagnosticsReport()
        {
            const string source = @"
using System;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        private void Start()
        {
            IntPtr ptr;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonExposedArrayVariableHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            Transform[] t;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonExposedPrimitiveVariableHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            int i = 0;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonExposedVariableHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            Transform t;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonNotExposedVariableHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportVariablesOfType.ComponentId)
                             .WithLocation(12, 13)
                             .WithSeverity(DiagnosticSeverity.Error)
                             .WithArguments("System.IntPtr");

            const string source = @"
using System;

using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            IntPtr ptr;
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}