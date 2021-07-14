using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotAllowDirectlySetValueWithFieldChangeCallbackTest : DiagnosticVerifier<NotAllowDirectlySetValueWithFieldChangeCallback>
    {
        [Fact]
        public async Task UdonSharpBehaviourDirectlySetFieldValueWithFieldChangeCallbackInAnotherClassHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotAllowDirectlySetValueWithFieldChangeCallback.ComponentId)
                .WithSeverity(DiagnosticSeverity.Error);

            const string source = @"
using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [SerializeField]
        private TestBehaviour _behaviour;

        [FieldChangeCallback(nameof(Hello))]
        public string bkHello;

        public string Hello
        {
            set => bkHello = value;
            get => bkHello;
        }

        private void Start()
        {
            [|_behaviour.bkHello = ""Test""|];
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourDirectlySetFieldValueWithFieldChangeCallbackInSameClassHasDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [FieldChangeCallback(nameof(Hello))]
        public string bkHello;

        public string Hello
        {
            set => bkHello = value;
            get => bkHello;
        }

        private void Start()
        {
            bkHello = ""Hello"";
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }
    }
}