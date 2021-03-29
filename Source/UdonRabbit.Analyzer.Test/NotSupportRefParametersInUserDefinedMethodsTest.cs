using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportRefParametersInUserDefinedMethodsTest : DiagnosticVerifier<NotSupportRefParametersInUserDefinedMethods>
    {
        [Fact]
        public async Task MonoBehaviourRefParameterInMethodDeclarationHasNoDiagnosticReport()
        {
            const string source = @"
using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        public void TestMethod(ref string str)
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourRefParameterInMethodDeclarationHasDiagnosticReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportRefParametersInUserDefinedMethods.ComponentId)
                             .WithLocation(8, 32)
                             .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        public void TestMethod(ref string str)
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourRefParameterInMethodParametersHasNoDiagnosticReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportRefParametersInUserDefinedMethods.ComponentId)
                             .WithLocation(14, 33)
                             .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            var s = """";
            SomeMethod(ref s);
        }

        private void SomeMethod(ref string str)
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}