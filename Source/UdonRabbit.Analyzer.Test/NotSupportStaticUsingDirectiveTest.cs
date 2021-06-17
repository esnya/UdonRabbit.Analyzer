using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportStaticUsingDirectiveTest : CodeFixVerifier<NotSupportStaticUsingDirective, NotSupportStaticUsingDirectiveCodeFixProvider>
    {
        [Fact]
        public async Task MonoBehaviourStaticUsingDirectiveHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

using static UnityEngine.Debug;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        private void Start()
        {
            Log(""Hello"");
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourStaticUsingDirectiveAlreadyHasParentUsingDirectiveHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportStaticUsingDirective.ComponentId)
                .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

using UnityEngine;

[|using static UnityEngine.Debug;|]

//
namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [SerializeField]
        private string _field;

        private void Start()
        {
            Log(""Hello"");
            Break();
        }
    }
}
";

            const string newSource = @"
using UdonSharp;

using UnityEngine;

//
namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [SerializeField]
        private string _field;

        private void Start()
        {
            Debug.Log(""Hello"");
            Debug.Break();
        }
    }
}
";

            await VerifyCodeFixAsync(source, new[] { diagnostic }, newSource);
        }

        [Fact]
        public async Task UdonSharpBehaviourStaticUsingDirectiveHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportStaticUsingDirective.ComponentId)
                .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

[|using static UnityEngine.Debug;|]

//
namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            Log(""Hello"");
            Break();

            if (developerConsoleVisible)
                Log(""Is Console Visible"");
        }
    }
}
";

            const string newSource = @"
using UdonSharp;

using UnityEngine;

//
namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            Debug.Log(""Hello"");
            Debug.Break();

            if (Debug.developerConsoleVisible)
                Debug.Log(""Is Console Visible"");
        }
    }
}
";

            await VerifyCodeFixAsync(source, new[] { diagnostic }, newSource);
        }
    }
}