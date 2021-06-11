using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

// ReSharper disable StaticMemberInGenericType

namespace UdonRabbit.Analyzer.Test.Infrastructure
{
    public abstract class DiagnosticVerifier<TAnalyzer> where TAnalyzer : DiagnosticAnalyzer, new()
    {
        protected DiagnosticResult ExpectDiagnostic(string diagnosticId)
        {
            return new(new TAnalyzer().SupportedDiagnostics.Single(w => w.Id == diagnosticId));
        }

        protected async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
        {
            var testProject = new TestUnityProject<TAnalyzer>(expected.Select(w => w.Id).Distinct().ToArray());

            ParseSource(testProject, source, expected);

            await testProject.RunAnalyzerAsync(CancellationToken.None);
        }

        protected void ParseSource(TestUnityProject<TAnalyzer> testProject, string source, DiagnosticResult[] expected)
        {
            var sb = new StringBuilder();
            var diagnostics = expected.ToList();

            var line = 1;
            var column = 1;
            var expectedLine = 0;
            var expectedColumn = 0;
            var isReading = false;
            var i = 0;

            using var sr = new StringReader(source);
            while (sr.Peek() > -1)
            {
                var c = sr.Read();
                switch (c)
                {
                    case '\n':
                        sb.Append((char) c);
                        line++;
                        column = 1;
                        break;

                    case '[' when sr.Peek() == '|':
                        sr.Read();

                        expectedLine = line;
                        expectedColumn = column;
                        isReading = true;
                        break;

                    case '|' when isReading && sr.Peek() == ']':
                        sr.Read();

                        diagnostics[i] = diagnostics[i].WithSpan(expectedLine, expectedColumn, line, column);
                        i++;
                        isReading = false;
                        break;

                    default:
                        sb.Append((char) c);
                        column++;
                        break;
                }
            }

            testProject.ExpectedDiagnostics.AddRange(diagnostics);
            testProject.SourceCode = sb.ToString();
        }
    }
}