using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using UdonRabbit.Analyzer.Udon;

namespace UdonRabbit.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NotSupportIsPatternKeywordInExpression : DiagnosticAnalyzer
    {
        public const string ComponentId = "URA0022";
        private const string Category = UdonConstants.UdonCategory;
        private const string HelpLinkUri = "https://github.com/esnya/UdonRabbit.Analyzer/blob/master/docs/analyzers/URA0022.md";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.URA0022Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.URA0022MessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.URA0022Description), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor RuleSet = new(ComponentId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description, HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleSet);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeIsPatternExpression, SyntaxKind.IsPatternExpression);
        }

        private static void AnalyzeIsPatternExpression(SyntaxNodeAnalysisContext context)
        {
            var expression = (IsPatternExpressionSyntax) context.Node;
            if (!UdonSharpBehaviourUtility.ShouldAnalyzeSyntax(context.SemanticModel, expression))
                return;

            UdonSharpBehaviourUtility.ReportDiagnosticsIfValid(context, RuleSet, expression);
        }
    }
}