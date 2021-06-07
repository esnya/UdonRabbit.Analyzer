using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

using Xunit;

// ReSharper disable StaticMemberInGenericType

namespace UdonRabbit.Analyzer.Test.Infrastructure
{
    public abstract class DiagnosticVerifier<TAnalyzer> where TAnalyzer : DiagnosticAnalyzer, new()
    {
        protected DiagnosticResult ExpectDiagnostic(string diagnosticId)
        {
            return new(new TAnalyzer().SupportedDiagnostics.Single(w => w.Id == diagnosticId));
        }

        protected DiagnosticResult ExpectDiagnostic(DiagnosticAnalyzer analyzer, string diagnosticId)
        {
            return new(analyzer.SupportedDiagnostics.First(w => w.Id == diagnosticId));
        }

        protected async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
        {
            var testProject = new TestUnityProject(expected.Select(w => w.Id).Distinct().ToArray());

            ParseSource(testProject, source, expected);

            await testProject.RunAsync(CancellationToken.None);
        }

        private void ParseSource(TestUnityProject testProject, string source, DiagnosticResult[] expected)
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

        // Maybe CSharpAnalyzerTest isn't work in Unity Project????
        private class TestUnityProject
        {
            private const string TestProjectId = "TestUnityProject";
            private const string EnvProject = "UDONRABBIT_ANALYZER_TEST_PROJECT";
            private const string EnvUdonSharp = "UDONRABBIT_ANALYZER_TEST_UDON_SHARP";

            private static readonly string TestProjectPath = Environment.GetEnvironmentVariable(EnvProject);
            private static readonly string UdonSharpPath = Environment.GetEnvironmentVariable(EnvUdonSharp);

            private static readonly HashSet<string> AllowedDiagnostics = new()
            {
                "CS0162", // https://docs.microsoft.com/ja-jp/dotnet/csharp/misc/cs0162
                "CS0164", // https://docs.microsoft.com/ja-jp/dotnet/csharp/misc/cs0164
                "CS0168", // https://docs.microsoft.com/ja-jp/dotnet/csharp/misc/cs0168
                "CS0169", // https://docs.microsoft.com/ja-jp/dotnet/csharp/misc/cs0169
                "CS0219", // https://docs.microsoft.com/ja-jp/dotnet/csharp/misc/cs0219
                "CS0414", // https://docs.microsoft.com/ja-jp/dotnet/csharp/misc/cs0414
                "CS0649", // https://docs.microsoft.com/ja-jp/dotnet/csharp/misc/cs0649
                "CS1701" // https://docs.microsoft.com/ja-jp/dotnet/csharp/language-reference/compiler-messages/cs1701
            };

            private readonly Solution _solution;

            public readonly List<DiagnosticResult> ExpectedDiagnostics = new();

            public string SourceCode { get; set; }

            static TestUnityProject()
            {
                // Workaround for configuring environment variables in xUnit
                using var sr = new StreamReader("./UdonRabbit.runsettings");
                var document = new XPathDocument(sr);
                var navigator = document.CreateNavigator();

                void ConfigureProcessEnvironmentVariableIfNotExists(string env, ref string variable)
                {
                    if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(env)))
                        return;

                    var query = navigator.Compile($"//EnvironmentVariables/{env}");
                    variable = navigator.SelectSingleNode(query)?.Value;
                }

                ConfigureProcessEnvironmentVariableIfNotExists(EnvProject, ref TestProjectPath);
                ConfigureProcessEnvironmentVariableIfNotExists(EnvUdonSharp, ref UdonSharpPath);
            }

            public TestUnityProject(params string[] ids)
            {
                var projectId = ProjectId.CreateNewId(TestProjectId);
                var solution = new AdhocWorkspace().CurrentSolution.AddProject(projectId, TestProjectId, TestProjectId, LanguageNames.CSharp);

                // emulate Unity C# project references
                solution = EmulateUnityExternalReferences().Aggregate(solution, (sol, path) => sol.AddMetadataReference(projectId, MetadataReference.CreateFromFile(path)));
                solution = EmulateUnityProjectReferences().Aggregate(solution, (sol, path) => sol.AddMetadataReference(projectId, MetadataReference.CreateFromFile(path)));

                // compiler options
                var compilerOptions = solution.GetProject(projectId)?.CompilationOptions;
                if (compilerOptions != null)
                {
                    compilerOptions = ids.Aggregate(compilerOptions, (options, id) => options.WithSpecificDiagnosticOptions(options.SpecificDiagnosticOptions.Add(id, ReportDiagnostic.Error)));
                    solution = solution.WithProjectCompilationOptions(projectId, compilerOptions!);
                }

                _solution = solution;
            }

            public async Task RunAsync(CancellationToken cancellationToken)
            {
                const string filename = "TestBehaviour.cs";

                var analyzer = new TAnalyzer();
                var diagnostics = new List<Diagnostic>();
                var documentId = DocumentId.CreateNewId(_solution.ProjectIds.First(), filename);
                var solution = _solution.AddDocument(documentId, filename, SourceText.From(SourceCode), filePath: $"/{filename}");
                var project = solution.GetProject(solution.ProjectIds.First());
                Assert.NotNull(project);

                var compilation = await project.GetCompilationAsync(cancellationToken);
                Assert.NotNull(compilation);

                var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, true);
                var compilationWithAnalyzers = compilation.WithOptions(compilationOptions)
                                                          .WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));

                var allDiagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync(cancellationToken);
                var ignores = allDiagnostics.Where(w => AllowedDiagnostics.Contains(w.Id));

                foreach (var diagnostic in allDiagnostics.Except(ignores).Where(w => w.Location.IsInSource).ToList())
                    if (diagnostic.Location == Location.None || diagnostic.Location.IsInMetadata)
                    {
                        diagnostics.Add(diagnostic);
                    }
                    else
                    {
                        var syntax = await project.Documents.First().GetSyntaxTreeAsync(cancellationToken);
                        if (syntax == diagnostic.Location.SourceTree)
                            diagnostics.Add(diagnostic);
                    }

                VerifyDiagnosticsResults(diagnostics.ToArray(), ExpectedDiagnostics.ToArray());
            }

            // ref: https://github.com/microsoft/Microsoft.Unity.Analyzers/blob/1.10.0/src/Microsoft.Unity.Analyzers.Tests/Infrastructure/DiagnosticVerifier.cs#L85
            private static void VerifyDiagnosticsResults(Diagnostic[] actualResults, params DiagnosticResult[] expectedResults)
            {
                if (expectedResults.Length != actualResults.Length)
                    Assert.True(false, $"Mismatch between number of diagnostics returned, expected {expectedResults.Length} actual {actualResults.Length}.");

                for (var i = 0; i < expectedResults.Length; i++)
                {
                    var actual = actualResults[i];
                    var expected = expectedResults[i];

                    if (!expected.HasLocation)
                    {
                        if (actual.Location != Location.None)
                            Assert.True(false, "Expected: A project diagnostic with No location; Actual: Has location");
                    }
                    else
                    {
                        VerifyDiagnosticLocations(actual.Location, expected.Spans);
                    }

                    if (actual.Id != expected.Id)
                        Assert.True(false, $"Expected diagnostic ID to be {expected.Id}, but was {actual.Id}");

                    if (actual.Severity != expected.Severity)
                        Assert.True(false, $"Expected diagnostic severity to be {expected.Severity}, but was {actual.Severity}");

                    if (actual.GetMessage() != expected.Message)
                        Assert.True(false, $"Expected diagnostic message to be {expected.Message}, but was {actual.GetMessage()}");
                }
            }

            private static void VerifyDiagnosticLocations(Location actual, IEnumerable<DiagnosticLocation> locations)
            {
                var actualSpan = actual.GetLineSpan();
                var expected = locations.First();

                var actualStartLinePosition = actualSpan.StartLinePosition;
                if (actualStartLinePosition.Line > 0 && actualStartLinePosition.Line != expected.Span.StartLinePosition.Line)
                    Assert.True(false, $"Expected diagnostic to start on line {expected.Span.StartLinePosition.Line + 1}, but was actually on line {actualStartLinePosition.Line + 1}");

                if (actualStartLinePosition.Character > 0 && actualStartLinePosition.Character != expected.Span.StartLinePosition.Character)
                    Assert.True(false, $"Expected diagnostic to start at column {expected.Span.StartLinePosition.Character}, but was actually at column {actualStartLinePosition.Character}");

                var actualEndLinePosition = actualSpan.EndLinePosition;
                if (actualEndLinePosition.Line > 0 && actualEndLinePosition.Line != expected.Span.EndLinePosition.Line)
                    Assert.True(false, $"Expected diagnostic to end on line {expected.Span.EndLinePosition.Line + 1}, but was actually on line {actualEndLinePosition.Line + 1}");

                if (actualEndLinePosition.Character > 0 && actualEndLinePosition.Character != expected.Span.EndLinePosition.Character)
                    Assert.True(false, $"Expected diagnostic to end at column {expected.Span.EndLinePosition.Character}, but was actually at column {actualEndLinePosition.Character}");
            }

            private static IEnumerable<string> EmulateUnityExternalReferences()
            {
                using var sr = new StreamReader(TestProjectPath);
                var references = new List<string>();
                var document = new XPathDocument(sr);
                var navigator = document.CreateNavigator();
                var @namespace = new XmlNamespaceManager(navigator.NameTable);
                @namespace.AddNamespace("msbuild", "http://schemas.microsoft.com/developer/msbuild/2003");

                var node = navigator.Select("//msbuild:HintPath", @namespace);

                while (node.MoveNext())
                    references.Add(node.Current?.Value);

                return references;
            }

            private static IEnumerable<string> EmulateUnityProjectReferences()
            {
                yield return UdonSharpPath;
                yield return Path.Combine(Path.GetDirectoryName(UdonSharpPath), "VRC.Udon.dll");
            }
        }
    }
}