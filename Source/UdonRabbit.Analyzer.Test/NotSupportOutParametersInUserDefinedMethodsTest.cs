using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportOutParametersInUserDefinedMethodsTest : DiagnosticVerifier<NotSupportOutParametersInUserDefinedMethods>
    {
        [Fact]
        public async Task MonoBehaviourOutParameterInMethodDeclarationHasNoDiagnosticReport()
        {
            const string source = @"
using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        public void TestMethod(out string str)
        {
            str = """";
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourOutParameterInMethodDeclarationHasDiagnosticReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportOutParametersInUserDefinedMethods.ComponentId)
                .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        public void TestMethod([|out string str|])
        {
            str = """";
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourOutParameterInMethodParametersHasNoDiagnosticReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            var s = ""1"";
            int i;

            var b = int.TryParse(s, out i);
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }
    }
}