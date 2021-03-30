using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using UdonRabbit.Analyzer.Udon;

namespace UdonRabbit.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NotSupportIsKeywordInExpression : DiagnosticAnalyzer
    {
        public const string ComponentId = "URA0020";
        private const string Category = UdonConstants.UdonCategory;
        private const string HelpLinkUri = "https://github.com/esnya/UdonRabbit.Analyzer/blob/master/docs/analyzers/URA0020.md";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.URA0020Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.URA0020MessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.URA0020Description), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor RuleSet = new(ComponentId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description, HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleSet);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeIsExpression, SyntaxKind.IsExpression);
            context.RegisterSyntaxNodeAction(AnalyzeIsPatternExpression, SyntaxKind.IsPatternExpression);
        }

        private static void AnalyzeIsExpression(SyntaxNodeAnalysisContext context)
        {
            var expression = (BinaryExpressionSyntax) context.Node;
            if (!UdonSharpBehaviourUtility.ShouldAnalyzeSyntax(context.SemanticModel, expression))
                return;

            if (expression.Kind() == SyntaxKind.IsExpression)
                context.ReportDiagnostic(Diagnostic.Create(RuleSet, expression.GetLocation()));
        }

        private static void AnalyzeIsPatternExpression(SyntaxNodeAnalysisContext context)
        {
            var expression = (IsPatternExpressionSyntax) context.Node;
            if (!UdonSharpBehaviourUtility.ShouldAnalyzeSyntax(context.SemanticModel, expression))
                return;

            context.ReportDiagnostic(Diagnostic.Create(RuleSet, expression.GetLocation()));
        }
    }
}