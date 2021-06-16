using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using UdonRabbit.Analyzer.Udon;

namespace UdonRabbit.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NotSupportTypeofInUserDefinedTypes : DiagnosticAnalyzer
    {
        public const string ComponentId = "URA0029";
        private const string Category = UdonConstants.UdonSharpCategory;
        private const string HelpLinkUri = "https://github.com/esnya/UdonRabbit.Analyzer/blob/master/docs/analyzers/URA0029.md";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.URA0029Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.URA0029MessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.URA0029Description), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor RuleSet = new(ComponentId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description, HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleSet);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeTypeofExpression, SyntaxKind.TypeOfExpression);
        }

        private static void AnalyzeTypeofExpression(SyntaxNodeAnalysisContext context)
        {
            var expression = (TypeOfExpressionSyntax) context.Node;
            if (!UdonSharpBehaviourUtility.ShouldAnalyzeSyntax(context.SemanticModel, expression))
                return;

            var typeSymbol = context.SemanticModel.GetTypeInfo(expression.Type);
            if (UdonSharpBehaviourUtility.IsUserDefinedTypes(context.SemanticModel, typeSymbol.Type, typeSymbol.Type.TypeKind) && typeSymbol.Type.TypeKind != TypeKind.Array)
                UdonSharpBehaviourUtility.ReportDiagnosticsIfValid(context, RuleSet, expression);
        }
    }
}