using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportGotoTest : DiagnosticVerifier<NotSupportGoto>
    {
        [Fact]
        public async Task MonoBehaviourGotoCaseStatementHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        private void Start()
        {
            var str = ""a"";
            switch (str)
            {
                case ""a"":
                    break;

                case ""b"":
                    goto case ""a"";
                    break;
            }
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task MonoBehaviourGotoStatementHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        private void Start()
        {
            for (var i = 0; i < 100; i++)
            {
                for (var j = 0; j < 100; j++)
                {
                    if (i + j >= 50)
                        goto Break;
                }
            }

    Break:
            Debug.Log(""Breaking!"");
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourGotoCaseStatementHasDiagnosticsReport()
        {
            var diagnostic1 = ExpectDiagnostic(NotSupportGoto.ComponentId)
                              .WithLocation(17, 21)
                              .WithSeverity(DiagnosticSeverity.Error);

            var diagnostic2 = ExpectDiagnostic(NotSupportGoto.ComponentId)
                              .WithLocation(21, 21)
                              .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            var str = ""a"";
            switch (str)
            {
                case ""a"":
                    break;

                case ""b"":
                    goto case ""a"";
                    break;

                case ""c"":
                    goto default;
                    break;

                default:
                    break;
            }
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic1, diagnostic2);
        }

        [Fact]
        public async Task UdonSharpBehaviourGotoStatementHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportGoto.ComponentId)
                             .WithLocation(17, 25)
                             .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            for (var i = 0; i < 100; i++)
            {
                for (var j = 0; j < 100; j++)
                {
                    if (i + j >= 50)
                        goto Break;
                }
            }

    Break:
            Debug.Log(""Breaking!"");
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}