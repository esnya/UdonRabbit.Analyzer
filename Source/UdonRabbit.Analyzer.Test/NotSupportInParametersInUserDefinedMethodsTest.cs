using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportInParametersInUserDefinedMethodsTest : DiagnosticVerifier<NotSupportInParametersInUserDefinedMethods>
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
        public void TestMethod(in string str)
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourOutParameterInMethodDeclarationHasDiagnosticReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportInParametersInUserDefinedMethods.ComponentId)
                             .WithLocation(8, 32)
                             .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        public void TestMethod(in string str)
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}