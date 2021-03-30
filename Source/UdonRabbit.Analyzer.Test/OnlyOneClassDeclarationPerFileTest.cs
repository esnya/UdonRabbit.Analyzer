using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class OnlyOneClassDeclarationPerFileTest : DiagnosticVerifier<OnlyOneClassDeclarationPerFile>
    {
        [Fact]
        public async Task MonoBehaviourTwiceClassHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

namespace UdonRabbit
{
    public class SomeBehaviour1 : MonoBehaviour {}
    public class SomeBehaviour2 : MonoBehaviour {}
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourTwiceClassHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(OnlyOneClassDeclarationPerFile.ComponentId)
                             .WithLocation(7, 5)
                             .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class SomeBehaviour1 : UdonSharpBehaviour {}
    public class SomeBehaviour2 : UdonSharpBehaviour {}
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourTwiceClassInSeparatedNamespaceHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(OnlyOneClassDeclarationPerFile.ComponentId)
                             .WithLocation(10, 9)
                             .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class SomeBehaviour1 : UdonSharpBehaviour {}

    namespace Interop
    {
        public class SomeBehaviour2 : UdonSharpBehaviour {}
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourTwiceClassInSeparatedNamespaceInRootHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(OnlyOneClassDeclarationPerFile.ComponentId)
                             .WithLocation(11, 5)
                             .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class SomeBehaviour1 : UdonSharpBehaviour {}
}

namespace RabbitUdon
{
    public class SomeBehaviour2 : UdonSharpBehaviour {}
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }
    }
}