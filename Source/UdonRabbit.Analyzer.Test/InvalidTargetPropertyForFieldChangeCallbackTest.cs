using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class InvalidTargetPropertyForFieldChangeCallbackTest : DiagnosticVerifier<InvalidTargetPropertyForFieldChangeCallback>
    {
        [Fact]
        public async Task UdonSharpBehaviourSpecifyDeclaredPropertyInFieldChangeCallbackHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [FieldChangeCallback(""Property"")]
        public string _bkProperty;

        public string Property { get; set; }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourSpecifyNoDeclaredPropertyInFieldChangeCallbackHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(InvalidTargetPropertyForFieldChangeCallback.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Error)
                             .WithArguments("TestBehaviour");

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [|[FieldChangeCallback(""Property"")]
        public string _bkProperty;|]
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}