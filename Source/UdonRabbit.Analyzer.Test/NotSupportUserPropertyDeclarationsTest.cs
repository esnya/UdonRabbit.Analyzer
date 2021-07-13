using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportUserPropertyDeclarationsTest : DiagnosticVerifier<NotSupportUserPropertyDeclarations>
    {
        [Fact]
        public async Task MonoBehaviourClassUserPropertyDeclarationsHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

namespace UdonRabbit
{
    public class TestClass : MonoBehaviour
    {
        public string TestProperty { get; set; }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourClassUserGetPropertyDeclarationsHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportUserPropertyDeclarations.ComponentId)
                .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        [|public string TestProperty { get; }|]
    }
}
";

            DisableVerifierOn("0.20.0", Comparision.GreaterThanOrEqual);
            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourClassUserPropertyDeclarationsHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportUserPropertyDeclarations.ComponentId)
                .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        [|public string TestProperty { get; set; }|]
    }
}
";

            DisableVerifierOn("0.20.0", Comparision.GreaterThanOrEqual);
            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}