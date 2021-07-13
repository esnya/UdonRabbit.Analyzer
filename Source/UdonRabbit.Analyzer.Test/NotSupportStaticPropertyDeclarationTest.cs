using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportStaticPropertyDeclarationTest : DiagnosticVerifier<NotSupportStaticPropertyDeclaration>
    {
        [Fact]
        public async Task InstancePropertyDeclarationHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        public string SomeProperty { get; set; }
    }
}
";

            DisableVerifierOn("0.20.0", Comparision.LesserThan);
            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviour__HasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportStaticPropertyDeclaration.ComponentId)
                .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [|public static string SomeProperty { get; set; }|]
    }
}
";

            DisableVerifierOn("0.20.0", Comparision.LesserThan);
            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}