using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportNamespaceAliasDirectiveTest : DiagnosticVerifier<NotSupportNamespaceAliasDirective>
    {
        [Fact]
        public async Task MonoBehaviourNamespaceAliasDirectiveHasNoDiagnosticsReport()
        {
            const string source = @"
using U = UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : U.MonoBehaviour
    {
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourNamespaceAliasDirectiveHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportNamespaceAliasDirective.ComponentId)
                             .WithLocation(2, 1)
                             .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using USharp = UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : USharp.UdonSharpBehaviour
    {
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}