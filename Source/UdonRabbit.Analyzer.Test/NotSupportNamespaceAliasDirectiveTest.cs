using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportNamespaceAliasDirectiveTest : CodeFixVerifier<NotSupportNamespaceAliasDirective, NotSupportNamespaceAliasDirectiveCodeFixProvider>
    {
        [Fact]
        public async Task MonoBehaviourNamespaceAliasDirectiveHasNoDiagnosticsReport()
        {
            const string source = @"
using U = UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : U.MonoBehaviour
    {
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourNamespaceAliasDirectiveAlreadyDeclaredSameTypeNameHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportNamespaceAliasDirective.ComponentId)
                .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using System.Diagnostics;

[|using USharp = UdonSharp;|]
[|using U = UnityEngine;|]

//
namespace UdonRabbit
{
    public class TestBehaviour : USharp.UdonSharpBehaviour
    {
        [U::SerializeField]
        private string _value;

        private void Start()
        {
            Debug.WriteLine(""Hello, World"");

            var value = U.Mathf.PI * U::Mathf.Abs(-1.25f);
        }
    }
}
";

            const string newSource = @"
using System.Diagnostics;

using UdonSharp;

//
namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [UnityEngine.SerializeField]
        private string _value;

        private void Start()
        {
            Debug.WriteLine(""Hello, World"");

            var value = UnityEngine.Mathf.PI * UnityEngine.Mathf.Abs(-1.25f);
        }
    }
}
";

            await VerifyCodeFixAsync(source, new[] { diagnostic, diagnostic }, newSource);
        }

        [Fact]
        public async Task UdonSharpBehaviourNamespaceAliasDirectiveHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportNamespaceAliasDirective.ComponentId)
                .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
[|using USharp = UdonSharp;|]
[|using U = UnityEngine;|]

//
namespace UdonRabbit
{
    public class TestBehaviour : USharp.UdonSharpBehaviour
    {
        [U::SerializeField]
        private string _value;
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
        private string _value;
    }
}
";

            await VerifyCodeFixAsync(source, new[] { diagnostic, diagnostic }, newSource);
        }
    }
}