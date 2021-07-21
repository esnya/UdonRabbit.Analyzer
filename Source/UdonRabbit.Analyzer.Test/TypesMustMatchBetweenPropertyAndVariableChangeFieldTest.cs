using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class TypesMustMatchBetweenPropertyAndVariableChangeFieldTest : DiagnosticVerifier<TypesMustMatchBetweenPropertyAndVariableChangeField>
    {
        [Fact]
        public async Task UdonSharpBehaviourTypesMatchBetweenFieldCallbackAndPropertyHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [FieldChangeCallback(""Field"")]
        public string _field;

        public string Field { get; set; }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourTypesNotMatchBetweenFieldCallbackAndPropertyHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(TypesMustMatchBetweenPropertyAndVariableChangeField.ComponentId)
                .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [|[FieldChangeCallback(""Field"")]
        public string _field;|]

        public object Field { get; set; }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}