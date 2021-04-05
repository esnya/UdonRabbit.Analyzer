using CommandLine;

namespace UdonRabbit.Analyzer.CodeGen
{
    public class CommandLineOptions
    {
        [Option('i', "identifier", Required = true, HelpText = "Identifier of Analyzer")]
        public int DiagnosticId { get; set; }

        [Option('l', "class", Required = true, HelpText = "Class Name of Analyzer")]
        public string Class { get; set; }

        [Option('t', "title", Required = true, HelpText = "Title of Analyzer")]
        public string Title { get; set; }

        [Option('d', "description", Required = true, HelpText = "Description of Analyzer")]
        public string Description { get; set; }

        [Option('m', "message-format", Required = true, HelpText = "Message Format of Analyzer")]
        public string MessageFormat { get; set; }

        [Option('c', "category", Required = true, HelpText = "Category of Analyzer")]
        public AnalyzerCategory Category { get; set; }

        [Option('s', "severity", Required = false, Default = AnalyzerSeverity.Error, HelpText = "Severity of Analyzer")]
        public AnalyzerSeverity Severity { get; set; }

        [Option('w', "working-directory", Required = false)]
        public string WorkingDirectory { get; set; }
    }
}