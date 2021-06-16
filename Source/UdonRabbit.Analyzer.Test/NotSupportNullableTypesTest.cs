using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportNullableTypesTest : DiagnosticVerifier<NotSupportNullableTypes>
    {
        [Fact]
        public async Task MonoBehaviourNullableTypeDeclarationHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        private int? _i;

        private bool? TestMethod(bool? b)
        {
            return null;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourNonNullableTypeHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private int _i;

        private string TestMethod(bool b)
        {
            return null;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourNullableTypeHasDiagnosticsReport()
        {
            var diagnostic1 = ExpectDiagnostic(NotSupportNullableTypes.ComponentId)
                .WithSeverity(DiagnosticSeverity.Error);

            var diagnostic2 = ExpectDiagnostic(NotSupportNullableTypes.ComponentId)
                .WithSeverity(DiagnosticSeverity.Error);

            var diagnostic3 = ExpectDiagnostic(NotSupportNullableTypes.ComponentId)
                .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private [|int?|] _i;

        private [|bool?|] TestMethod([|bool?|] b)
        {
            return null;
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic1, diagnostic2, diagnostic3);
        }
    }
}