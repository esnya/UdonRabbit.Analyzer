using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportThrowingExceptionsTest : DiagnosticVerifier<NotSupportThrowingExceptions>
    {
        [Fact]
        public async Task MonoBehaviourClassThrowExpressionHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        private void Update()
        {
            object o = null;

            try
            {
                var i = (int) o;
            }
            catch
            {
                throw;
            }
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task MonoBehaviourClassThrowStatementHasNoDiagnosticsReport()
        {
            const string source = @"
using System;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        private void Update()
        {
            throw new InvalidOperationException();
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourThrowExpressionHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportThrowingExceptions.ComponentId)
                             .WithLocation(18, 17)
                             .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Update()
        {
            object o = null;

            try
            {
                var i = (int) o;
            }
            catch
            {
                throw;
            }
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourThrowStatementHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportThrowingExceptions.ComponentId)
                             .WithLocation(12, 13)
                             .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using System;

using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Update()
        {
            throw new InvalidOperationException();
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}