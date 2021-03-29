using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportMultidimensionalArrayAccessTest : DiagnosticVerifier<NotSupportMultidimensionalArrayAccess>
    {
        [Fact]
        public async Task MonoBehaviourMultidimensionalArrayAccessHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        private int[,] array1 = new int[2, 4];

        private void Start()
        {
            array1[0, 0] = 0;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourMultidimensionalArrayAccessHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportMultidimensionalArrayAccess.ComponentId)
                             .WithLocation(12, 13)
                             .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private int[,] array1 = new int[2, 4];

        private void Start()
        {
            array1[0, 0] = 1;
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourNormalArrayAccessHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private int[] array1 = new int[2];

        private void Start()
        {
            array1[0] = 1;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }
    }
}