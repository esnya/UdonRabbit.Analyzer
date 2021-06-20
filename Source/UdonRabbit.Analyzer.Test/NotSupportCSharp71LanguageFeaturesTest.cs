using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportCSharp71LanguageFeaturesTest : DiagnosticVerifier<NotSupportCSharp71LanguageFeatures>
    {
        [Fact]
        public async Task MonoBehaviourCSharp71LanguageFeaturesDefaultStatementHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        private void Start()
        {
            int i = default;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourCSharp71LanguageFeaturesDefaultStatementHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportCSharp71LanguageFeatures.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Error)
                             .WithArguments("default literal");

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            int i = [|default|];
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}