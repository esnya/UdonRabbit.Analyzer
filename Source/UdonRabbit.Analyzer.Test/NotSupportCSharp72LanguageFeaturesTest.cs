using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportCSharp72LanguageFeaturesTest : DiagnosticVerifier<NotSupportCSharp72LanguageFeatures>
    {
        [Fact]
        public async Task MonoBehaviourCSharp72LanguageFeatureLeadingDigitSeparatorHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        private void Start()
        {
            var b = 0b_1111_0001;
            var d = 0x_0001_F408;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourCSharp72LanguageFeatureLeadingDigitSeparatorHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportCSharp72LanguageFeatures.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Error)
                             .WithArguments("leading digit separator");

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            var b0 = 0b1111_0001;
            var b1 = [|0b_1111_0001|];
            var b2 = [|0B_1111_0001|];
            var d0 = 0x0001_F408;
            var d1 = [|0x_0001_F408|];
            var d2 = [|0X_0001_F408|];
        }
    }
}
";

            await VerifyAnalyzerAsync(source, Enumerable.Repeat(diagnostic, 4).ToArray());
        }
    }
}