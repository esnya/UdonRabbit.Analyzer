using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportConstructorsTest : DiagnosticVerifier<NotSupportConstructors>
    {
        [Fact]
        public async Task ClassHasConstructorHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportConstructors.ComponentId)
                             .WithLocation(8, 9)
                             .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        public TestClass()
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task NoDiagnosticsReport()
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
    }
}