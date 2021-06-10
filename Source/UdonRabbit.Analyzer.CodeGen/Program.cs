using System;
using System.IO;
using System.Resources;
using System.Threading.Tasks;

using CommandLine;
using CommandLine.Text;

namespace UdonRabbit.Analyzer.CodeGen
{
    internal static class Program
    {
        [STAThread]
        private static async Task Main(string[] args)
        {
            var parsed = Parser.Default.ParseArguments<CommandLineOptions>(args);

            await parsed.MapResult(ProcessArguments, _ =>
            {
                var text = HelpText.AutoBuild(parsed);
                Console.WriteLine(text.ToString());

                return Task.CompletedTask;
            });
        }

        private static async Task ProcessArguments(CommandLineOptions options)
        {
            var cwd = string.IsNullOrWhiteSpace(options.WorkingDirectory) ? Directory.GetCurrentDirectory() : options.WorkingDirectory;
            if (!IsValidWorkingDirectory(cwd))
                return;

            await ModifyAnalyzerIndex(cwd, options.DiagnosticId, options.Title, options.Category.ToString(), options.Severity.ToString());
            await CreateAnalyzerDocument(cwd, options.DiagnosticId, options.Title, options.Description);

            await ModifyResourceManager(cwd, options.DiagnosticId, options.Title, options.Description, options.MessageFormat);
            await CreateAnalyzerClass(cwd, options.DiagnosticId, options.Class, options.Category.ToString(), options.Severity.ToString());
            await CreateTestClass(cwd, options.Class, options.Severity.ToString());

            Console.WriteLine("done");
        }

        private static bool IsValidWorkingDirectory(string path)
        {
            if (!Directory.Exists(Path.Combine(path, "docs", "analyzers")))
                return false;

            if (!Directory.Exists(Path.Combine(path, "Source", "UdonRabbit.Analyzer")))
                return false;

            if (!Directory.Exists(Path.Combine(path, "Source", "UdonRabbit.Analyzer.Test")))
                return false;

            return true;
        }

        private static async Task ModifyAnalyzerIndex(string path, int id, string title, string category, string severity)
        {
            var index = Path.Combine(path, "docs", "analyzers", "README.md");
            using var sr = new StreamReader(index);
            var markdown = await sr.ReadToEndAsync();
            sr.Close();

            var link = $"| [{title}](./URA{id:0000}.md)";
            var newLine = $"| URA{id:0000}     {link,-99} | {category} | {severity} |";

            await using var sw = new StreamWriter(index);
            await sw.WriteLineAsync($"{markdown}{newLine}{Environment.NewLine}");
        }

        private static async Task CreateAnalyzerDocument(string path, int id, string title, string description)
        {
            var src = Path.Combine(path, "docs", "analyzers", "URA9999.md");
            var dest = Path.Combine(path, "docs", "analyzers", $"URA{id:0000}.md");

            using var sr = new StreamReader(src);
            var content = await sr.ReadToEndAsync();

            await using var sw = new StreamWriter(dest);
            content = content.Replace("URA9999", $"URA{id:0000}");
            content = content.Replace("FIXME_TITLE", title);
            content = content.Replace("FIXME_DESCRIPTION", description);

            await sw.WriteLineAsync(content);
        }

        private static async Task ModifyResourceManager(string path, int id, string title, string description, string messageFormat)
        {
            var destResX = Path.Combine(path, "Source", "UdonRabbit.Analyzer", "Resources.resx");
            using var rr = new ResXResourceReader(destResX);
            var node = rr.GetEnumerator();

            using var rw = new ResXResourceWriter(destResX);
            while (node.MoveNext())
                rw.AddResource(node.Key?.ToString()!, node.Value?.ToString()!);

            rw.AddResource(new ResXDataNode($"URA{id:0000}Description", description));
            rw.AddResource(new ResXDataNode($"URA{id:0000}MessageFormat", messageFormat));
            rw.AddResource(new ResXDataNode($"URA{id:0000}Title", title));
            rw.Close();
        }

        private static async Task CreateAnalyzerClass(string path, int id, string @class, string category, string severity)
        {
            var source = $@"
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using UdonRabbit.Analyzer.Udon;

namespace UdonRabbit.Analyzer
{{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class {@class} : DiagnosticAnalyzer
    {{
        public const string ComponentId = ""URA{id:0000}"";
        private const string Category = UdonConstants.{category}Category;
        private const string HelpLinkUri = ""https://github.com/esnya/UdonRabbit.Analyzer/blob/master/docs/analyzers/URA{id:0000}.md"";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.URA{id:0000}Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.URA{id:0000}MessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.URA{id:0000}Description), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor RuleSet = new(ComponentId, Title, MessageFormat, Category, DiagnosticSeverity.{severity}, true, Description, HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleSet);

        public override void Initialize(AnalysisContext context)
        {{
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        }}
    }}
}}
";

            var dest = Path.Combine(path, "Source", "UdonRabbit.Analyzer", $"{@class}.cs");
            await using var sw = new StreamWriter(dest);
            await sw.WriteLineAsync(source);
        }

        private static async Task CreateTestClass(string path, string @class, string severity)
        {
            var source = $@"
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Test.Infrastructure;

using Xunit;

namespace UdonRabbit.Analyzer.Test
{{
    public class {@class}Test : DiagnosticVerifier<{@class}>
    {{
        [Fact]
        public async Task MonoBehaviour__HasNoDiagnosticsReport()
        {{
            const string source = @""
using UnityEngine;

namespace UdonRabbit
{{
    public class TestBehaviour : MonoBehaviour
    {{
    }}
}}
"";

            await VerifyAnalyzerAsync(source);
        }}

        [Fact]
        public async Task UdonSharpBehaviour__HasNoDiagnosticsReport()
        {{
            const string source = @""
using UdonSharp;

namespace UdonRabbit
{{
    public class TestBehaviour : UdonSharpBehaviour
    {{
    }}
}}
"";

            await VerifyAnalyzerAsync(source);
        }}

        [Fact]
        public async Task UdonSharpBehaviour__HasDiagnosticsReport()
        {{
            var diagnostic = ExpectDiagnostic({@class}.ComponentId)
                             .WithLocation(10, 33)
                             .WithSeverity(DiagnosticSeverity.{severity});

            const string source = @""
using UdonSharp;

namespace UdonRabbit
{{
    public class TestBehaviour : UdonSharpBehaviour
    {{
    }}
}}
"";

            await VerifyAnalyzerAsync(source);
        }}
    }}
}}

            ";

            var dest = Path.Combine(path, "Source", "UdonRabbit.Analyzer.Test", $"{@class}Test.cs");
            await using var sr = new StreamWriter(dest);
            await sr.WriteLineAsync(source);
        }
    }
}