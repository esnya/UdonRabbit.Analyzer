using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportSyncingOfTypeTest : DiagnosticVerifier<NotSupportSyncingOfType>
    {
        [Fact]
        public async Task MonoBehaviourUdonNotSupportSyncingTypeHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        [UdonSynced]
        private Transform _transform;
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonNotSupportSyncingTypeHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportSyncingOfType.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Error)
                             .WithArguments("UnityEngine.Transform");

            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [|[UdonSynced]
        private Transform _transform;|]
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonSupportSyncingPrimitiveTypeHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [UdonSynced]
        private int _data;
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonSupportSyncingTypeHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [UdonSynced]
        private Vector3 _vec3;
    }
}
";

            await VerifyAnalyzerAsync(source);
        }
    }
}