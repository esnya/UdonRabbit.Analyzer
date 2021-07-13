using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{
    public class NotSupportVariablesOfTypeTest : DiagnosticVerifier<NotSupportVariablesOfType>
    {
        [Fact]
        public async Task MonoBehaviourUdonNotExposedVariableHasNoDiagnosticsReport()
        {
            const string source = @"
using System;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : MonoBehaviour
    {
        private void Start()
        {
            IntPtr ptr;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAllowedJaggedArraysHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            Transform[][] t;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourAnotherUdonSharpBehaviourVariableHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;
namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            UdonSharpBehaviour behaviour;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourArrayOfAnotherUdonSharpBehaviourVariableHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            UdonSharpBehaviour[] behaviour;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourJaggedArraysOfUserDefinedTypeHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            TestBehaviour[][] t;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourNotAllowedJaggedArrayHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportVariablesOfType.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Error)
                             .WithArguments("System.IntPtr[][]");

            const string source = @"
using System;

using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            [|IntPtr[][] ptr|];
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonAllowedCollectionsHasNoDiagnosticsReport()
        {
            const string source = @"
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            IEnumerable<Transform> collection = GetComponentsInChildren<Transform>().ToList();
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonAllowedVariableTypeInPropertyHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using VRC.SDKBase;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private VRC_Pickup Pickup { get; set; }
    }
}
";

            DisableVerifierOn("0.20.0", Comparision.LesserThan);
            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonAllowedVrcTypesHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using VRC.SDKBase;

using UnityEngine;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [SerializeField]
        public VRC_Pickup _pickup;
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonBehaviourVariableHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

using VRC.Udon;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            UdonBehaviour behaviour;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonExposedArrayOfPrimitiveVariableHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            int[] i = new int[2];
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonExposedArrayVariableHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            Transform[] t;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonExposedPrimitiveVariableHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            int i = 0;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonExposedVariableHasNoDiagnosticsReport()
        {
            const string source = @"
using UnityEngine;

using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            Transform t;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonNotAllowedVariableTypeInPropertyHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportVariablesOfType.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Error)
                             .WithArguments("System.IntPtr");

            const string source = @"
using System;

using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        [|public IntPtr Ptr { get; set; }|]
    }
}
";

            DisableVerifierOn("0.20.0", Comparision.LesserThan);
            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonNotExposedVariableHasDiagnosticsReport()
        {
            var diagnostic = ExpectDiagnostic(NotSupportVariablesOfType.ComponentId)
                             .WithSeverity(DiagnosticSeverity.Error)
                             .WithArguments("System.IntPtr");

            const string source = @"
using System;

using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            [|IntPtr ptr|];
        }
    }
}
";

            await VerifyAnalyzerAsync(source, diagnostic);
        }

        [Fact]
        public async Task UdonSharpBehaviourUdonSharpBehaviourVariableHasNoDiagnosticsReport()
        {
            const string source = @"
using UdonSharp;

namespace UdonRabbit
{
    public class TestBehaviour : UdonSharpBehaviour
    {
        private void Start()
        {
            UdonSharpBehaviour behaviour;
        }
    }
}
";

            await VerifyAnalyzerAsync(source);
        }
    }
}