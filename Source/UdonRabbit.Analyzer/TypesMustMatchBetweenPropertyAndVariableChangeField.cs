using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using UdonRabbit.Analyzer.Extensions;
using UdonRabbit.Analyzer.Udon;

namespace UdonRabbit.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TypesMustMatchBetweenPropertyAndVariableChangeField : DiagnosticAnalyzer
    {
        public const string ComponentId = "URA0055";
        private const string Category = UdonConstants.UdonSharpCategory;
        private const string HelpLinkUri = "https://github.com/esnya/UdonRabbit.Analyzer/blob/master/docs/analyzers/URA0055.md";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.URA0055Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.URA0055MessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.URA0055Description), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor RuleSet = new(ComponentId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description, HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleSet);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
        }

        private static void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (FieldDeclarationSyntax) context.Node;
            if (!UdonSharpBehaviourUtility.ShouldAnalyzeSyntax(context.SemanticModel, declaration))
                return;

            if (!declaration.HasAttribute(context.SemanticModel, UdonConstants.UdonSharpFieldChangeCallbackFullName))
                return;

            var attribute = declaration.GetAttributes(context.SemanticModel, UdonConstants.UdonSharpFieldChangeCallbackFullName).First();
            var targetProperty = attribute.ArgumentList.Arguments.FirstOrDefault()?.Expression.ParseValue();

            var property = context.SemanticModel.LookupProperty(declaration, targetProperty);
            if (property == null)
                return;

            var t = context.SemanticModel.GetTypeInfo(declaration.Declaration.Type);
            if (t.Type.Equals(property.Type, SymbolEqualityComparer.Default))
                return;

            UdonSharpBehaviourUtility.ReportDiagnosticsIfValid(context, RuleSet, declaration);
        }
    }
}