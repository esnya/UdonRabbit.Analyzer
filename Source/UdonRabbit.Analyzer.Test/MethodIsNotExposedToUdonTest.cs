using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class MethodIsNotExposedToUdonTest : DiagnosticVerifier<MethodIsNotExposedToUdon>
    {
        [Fact]
        public async Task AllowedMethodIsNoDiagnosticsReport()
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
        public async Task NoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        private void Update()
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task NotAllowedFieldAccessorHasDiagnosticsReport()
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