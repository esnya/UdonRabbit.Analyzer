using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportConstructorsTest : DiagnosticVerifier<NotSupportConstructors>
    {
        [Fact]
        public async Task MonoBehaviourThatHasConstructorHasNoDiagnosticsReport()
        {
            const string source = @"
namespace UdonRabbit
{
    public class TestClass
    {
        public TestClass()
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourThatHasConstructorHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportConstructors.ComponentId)
                .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        [|public TestClass()
        {
        }|]
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}