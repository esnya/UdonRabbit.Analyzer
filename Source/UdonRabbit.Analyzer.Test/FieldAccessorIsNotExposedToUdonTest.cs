using System.Threading.Tasks;

using Xunit;

using VerifyCS = UdonRabbit.Analyzer.Test.Infrastructure.DiagnosticVerifier<UdonRabbit.Analyzer.FieldAccessorIsNotExposedToUdon>;

namespace UdonRabbit.Analyzer.Test
{
    public class FieldAccessorIsNotExposedToUdonTest
    {
        [Fact]
        public async Task AllowedMethodIsNoDiagnosticsReport()
        {
            var source = @"
using System;

using UdonSharp;

using UnityEngine;

namespace UdonRabbit
{
    public class TestClass : UdonSharpBehaviour
    {
        private void Update()
        {
            var str = new string[15];
            var newStr = string.Join("", "", str);
        }
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task NoDiagnosticsReport()
        {
            var source = @"
using UdonSharp;

using UnityEngine;

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

            await VerifyCS.VerifyAnalyzerAsync(source);
        }
    }
}