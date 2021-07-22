using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotAllowDirectlySetValueWithFieldChangeCallbackTest : CodeFixVerifier<NotAllowDirectlySetValueWithFieldChangeCallback, NotAllowDirectlySetValueWithFieldChangeCallbackCodeFixProvider>
    {
        [Fact]
        public async Task UdonSharpBehaviourDirectlySetFieldValueWithFieldChangeCallbackInAnotherClassThatHasNotPublicSetterHasDiagnosticsReport()
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
            private set => bkHello = value;
            get => bkHello;
        }

        private void Start()
        {
            [|_behaviour.bkHello = ""Test""|];
        }
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
        [SerializeField]
        private TestBehaviour _behaviour;

        [FieldChangeCallback(nameof(Hello))]
        public string bkHello;

        public string Hello
        {
            private set => bkHello = value;
            get => bkHello;
        }

        private void Start()
        {
            _behaviour.SetProgramVariable(""bkHello"", ""Test"");
        }
    }
}
";

            await VerifyCodeFixAsync(source, new[] { diagnostic }, newSource);
        }

        [Fact]
        public async Task UdonSharpBehaviourDirectlySetFieldValueWithFieldChangeCallbackInAnotherClassThatHasPublicSetterHasDiagnosticsReport()
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

            const string newSource = @"
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
            _behaviour.Hello = ""Test"";
        }
    }
}
";

            await VerifyCodeFixAsync(source, new[] { diagnostic }, newSource);
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