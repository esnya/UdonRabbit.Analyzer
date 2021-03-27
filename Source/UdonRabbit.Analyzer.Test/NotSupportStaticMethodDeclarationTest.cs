using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportStaticMethodDeclarationTest : DiagnosticVerifier<NotSupportStaticMethodDeclaration>
    {
        [Fact]
        public async Task InstanceMethodIsNoDiagnosticsReport()
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

        private void TestMethod()
        {
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
        public async Task StaticMethodHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportStaticMethodDeclaration.ComponentId)
                             .WithLocation(12, 9)
                             .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        private void Update()
        {
        }

        private static void TestMethod()
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}