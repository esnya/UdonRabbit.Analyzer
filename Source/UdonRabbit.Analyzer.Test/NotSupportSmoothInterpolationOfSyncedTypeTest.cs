using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportSmoothInterpolationOfSyncedTypeTest : CodeFixVerifier<NotSupportSmoothInterpolationOfSyncedType, NotSupportSmoothInterpolationOfSyncedTypeCodeFixProvider>
    {
        [Fact]
        public async Task UdonSharpBehaviourNotSupportSmoothInterpolationSyncTypeHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportSmoothInterpolationOfSyncedType.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Error)
                             .WithArguments("bool");

            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [|[UdonSynced(UdonSyncMode.Smooth), SerializeField]
        [Tooltip("""")]
        private bool _b;|]
    }
}
";

            const string newSource = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [UdonSynced, SerializeField]
        [Tooltip("""")]
        private bool _b;
    }
}
";

            await VerifyCodeFixAsync(source, new[] { diagnostic }, newSource);
        }

        [Fact]
        public async Task UdonSharpBehaviourSupportSmoothInterpolationSyncTypeHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [UdonSynced(UdonSyncMode.Smooth)]
        private float _b;
    }
}
";

            await VerifyAnalyzerAsync(source);
        }
    }
}