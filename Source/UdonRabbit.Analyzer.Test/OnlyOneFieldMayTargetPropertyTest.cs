using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class OnlyOneFieldMayTargetPropertyTest : DiagnosticVerifier<OnlyOneFieldMayTargetProperty>
    {
        [Fact]
        public async Task UdonSharpBehaviourFieldChangeCallbackHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [FieldChangeCallback(""SomeProperty"")]
        private string _foo;
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourFieldChangeCallbackInMultipleVariableDeclarationHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(OnlyOneFieldMayTargetProperty.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Error)
                             .WithArguments("SomeProperty");

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [|[FieldChangeCallback(""SomeProperty"")]
        private string _foo, _bar;|]
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourFieldChangeCallbackInSameTargetToMultipleVariableDeclarationHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(OnlyOneFieldMayTargetProperty.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Error)
                             .WithArguments("SomeProperty");

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [|[FieldChangeCallback(""SomeProperty"")]
        private string _foo;|]

        [|[FieldChangeCallback(""SomeProperty"")]
        private string _bar;|]

        [FieldChangeCallback(""BazProperty"")]
        private string _baz;
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic, diagnostic);
        }
    }
}