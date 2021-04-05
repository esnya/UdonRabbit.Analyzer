using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using UdonRabbit.Analyzer.Udon;

namespace UdonRabbit.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NotSupportNullableTypes : DiagnosticAnalyzer
    {
        public const string ComponentId = "URA0040";
        private const string Category = UdonConstants.UdonSharpCategory;
        private const string HelpLinkUri = "https://github.com/mika-f/UdonRabbit.Analyzer/blob/master/docs/analyzers/URA0040.md";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.URA0040Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.URA0040MessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.URA0040Description), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor RuleSet = new(ComponentId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description, HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleSet);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeNullableType, SyntaxKind.NullableType);
        }

        private static void AnalyzeNullableType(SyntaxNodeAnalysisContext context)
        {
            var nullable = (NullableTypeSyntax) context.Node;
            if (!UdonSharpBehaviourUtility.ShouldAnalyzeSyntax(context.SemanticModel, nullable))
                return;

            context.ReportDiagnostic(Diagnostic.Create(RuleSet, nullable.GetLocation()));
        }
    }
}