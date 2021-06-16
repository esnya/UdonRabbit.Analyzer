using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportAwakeTest : DiagnosticVerifier<NotSupportAwake>
    {
        [Fact]
        public async Task MonoBehaviourAwakeEventHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        private void Awake() {}
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAwakeEventHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportAwake.ComponentId)
                .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [|private void Awake() {}|]
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}