using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class OnlySupportsOneTypeGenericsTest : DiagnosticVerifier<OnlySupportsOneTypeGenerics>
    {
        [Fact]
        public async Task AllowedOneTypeGenericsIsNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        public GameObject go;

        private void Update()
        {
            go.GetComponent<Transform>();
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
            var diagnostic = ExpectDiagnostic(OnlySupportsOneTypeGenerics.ComponentId)
                             .WithLocation(14, 13)
                             .WithSeverity(DiagnosticSeverity.Error)
                             .WithArguments("2");

            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        public ParticleSystem go;

        private void Update()
        {
            GetComponentFor<Transform, Transform>();
        }

        private void GetComponentFor<T1, T2>()
        {
            go.GetComponent<T1>();
            go.GetComponent<T2>();
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}