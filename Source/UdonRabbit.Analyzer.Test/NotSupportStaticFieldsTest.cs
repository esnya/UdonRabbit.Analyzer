using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportStaticFieldsTest : DiagnosticVerifier<NotSupportStaticFields>
    {
        [Fact]
        public async Task MonoBehaviourStaticFieldsHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

namespace UdonRabbit
{
    public class TestClass : MonoBehaviour
    {
        public static string Message = ""Hello"";
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourStaticFieldsHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportStaticFields.ComponentId)
                             .WithLocation(8, 9)
                             .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        public static string Message = ""Hello"";
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}