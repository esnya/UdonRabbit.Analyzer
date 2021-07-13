using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportPropertyInitializersTest : DiagnosticVerifier<NotSupportPropertyInitializers>
    {
        [Fact]
        public async Task UdonSharpBehaviourPropertyDeclarationWithInitializerHasNoDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportPropertyInitializers.ComponentId)
                .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [|public string SomeProperty { get; } = ""Hello, World"";|]
    }
}
";

            DisableVerifierOn("0.20.0", Comparision.LesserThan);
            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourPropertyDeclarationWithoutInitializerHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        public string SomeProperty { get; }
    }
}
";

            DisableVerifierOn("0.20.0", Comparision.LesserThan);
            await VerifyAnalyzerAsync(source);
        }
    }
}