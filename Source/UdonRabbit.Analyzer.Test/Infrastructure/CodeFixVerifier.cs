using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace UdonRabbit.Analyzer.Test.Infrastructure
{
    public abstract class CodeFixVerifier<TAnalyzer, TCodeFix> : DiagnosticVerifier<TAnalyzer> where TAnalyzer : DiagnosticAnalyzer, new() where TCodeFix : CodeFixProvider, new()
    {
        protected async Task VerifyCodeFixAsync(string source, DiagnosticResult[] expected, string fixedSource)
        {
            var testProject = new TestUnityProject<TAnalyzer>(expected.Select(w => w.Id).Distinct().ToArray());

            ParseSource(testProject, source, expected);

            await testProject.RunAnalyzerAsync(CancellationToken.None);
            await testProject.RunCodeFixAsync<TCodeFix>(fixedSource, CancellationToken.None);
        }
    }
}