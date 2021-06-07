using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportInheritingFromInterfacesTest : DiagnosticVerifier<NotSupportInheritingFromInterfaces>
    {
        [Fact]
        public async Task InheritingFromInterfacesHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportInheritingFromInterfaces.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Error)
                             .WithArguments("IDisposable");

            const string source = @"
using System;

using UdonSharp;

namespace UdonRabbit
{
    [|public class TestClass : UdonSharpBehaviour, IDisposable
    {
        private void Update()
        {
        }

        public void Dispose()
        {
        }
    }|]
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UnityISerializationCallbackReceiverIsNoDiagnosticsReport()
        {
            // NOTE: UdonSharpBehaviour is inheriting from UnityEngine.ISerializationCallbackReceiver on Editor
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        private void Update()
        {
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }
    }
}