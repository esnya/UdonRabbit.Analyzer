using System.Threading.Tasks;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class FieldAccessorIsNotExposedToUdonTest : DiagnosticVerifier<FieldAccessorIsNotExposedToUdon>
    {
        [Fact]
        public async Task AllowedFieldAccessorIsNoDiagnosticsReport()
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
            var diagnostic = ExpectDiagnostic(FieldAccessorIsNotExposedToUdon.ComponentId)
                             .WithLocation(12, 23)
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