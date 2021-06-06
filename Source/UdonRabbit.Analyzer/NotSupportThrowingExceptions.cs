using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using UdonRabbit.Analyzer.Udon;

namespace UdonRabbit.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NotSupportThrowingExceptions : DiagnosticAnalyzer
    {
        public const string ComponentId = "URA0012";
        private const string Category = UdonConstants.UdonCategory;
        private const string HelpLinkUri = "https://github.com/esnya/UdonRabbit.Analyzer/blob/master/docs/analyzers/URA0012.md";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.URA0012Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.URA0012MessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.URA0012Description), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor RuleSet = new(ComponentId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description, HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleSet);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeThrowStatement, SyntaxKind.ThrowStatement);
            context.RegisterSyntaxNodeAction(AnalyzeThrowExpression, SyntaxKind.ThrowExpression);
        }

        private static void AnalyzeThrowStatement(SyntaxNodeAnalysisContext context)
        {
            var statement = (ThrowStatementSyntax) context.Node;
            if (!UdonSharpBehaviourUtility.ShouldAnalyzeSyntax(context.SemanticModel, statement))
                return;

            UdonSharpBehaviourUtility.ReportDiagnosticsIfValid(context, RuleSet, statement);
        }

        private static void AnalyzeThrowExpression(SyntaxNodeAnalysisContext context)
        {
            var statement = (ThrowExpressionSyntax) context.Node;
            if (!UdonSharpBehaviourUtility.ShouldAnalyzeSyntax(context.SemanticModel, statement))
                return;

            UdonSharpBehaviourUtility.ReportDiagnosticsIfValid(context, RuleSet, statement);
        }
    }
}