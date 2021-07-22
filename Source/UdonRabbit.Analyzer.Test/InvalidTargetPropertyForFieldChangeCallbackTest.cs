using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class InvalidTargetPropertyForFieldChangeCallbackTest : CodeFixVerifier<InvalidTargetPropertyForFieldChangeCallback, InvalidTargetPropertyForFieldChangeCallbackCodeFixProvider>
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

            DisableVerifierOn("0.20.0", Comparision.LesserThan);
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

            const string newSource = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [FieldChangeCallback(""Property"")]
        public string _bkProperty;

        public string Property
        {
            get => _bkProperty;
            set
            {
                _bkProperty = value;
            }
        }
    }
}
";

            DisableVerifierOn("0.20.0", Comparision.LesserThan);
            await VerifyCodeFixAsync(source, new[] { diagnostic }, newSource);
        }
    }
}