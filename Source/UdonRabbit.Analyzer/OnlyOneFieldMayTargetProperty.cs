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
    public class OnlyOneFieldMayTargetProperty : DiagnosticAnalyzer
    {
        public const string ComponentId = "URA0054";
        private const string Category = UdonConstants.UdonSharpCategory;
        private const string HelpLinkUri = "https://github.com/esnya/UdonRabbit.Analyzer/blob/master/docs/analyzers/URA0054.md";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.URA0054Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.URA0054MessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.URA0054Description), Resources.ResourceManager, typeof(Resources));
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

            bool IsFieldChangeCallbackAttribute(AttributeSyntax attr)
            {
                var t = context.SemanticModel.GetTypeInfo(attr);
                if (t.Type == null)
                    return false;
                return t.Type.Equals(context.SemanticModel.Compilation.GetTypeByMetadataName(UdonConstants.UdonSharpFieldChangeCallbackFullName), SymbolEqualityComparer.Default);
            }

            var hasFieldChangeCallbackAttribute = declaration.AttributeLists.SelectMany(w => w.Attributes).Any(IsFieldChangeCallbackAttribute);
            var attribute = declaration.AttributeLists.SelectMany(w => w.Attributes).First(IsFieldChangeCallbackAttribute);
            var targetProperty = attribute.ArgumentList.Arguments.FirstOrDefault()?.Expression.ParseValue();

            if (hasFieldChangeCallbackAttribute && declaration.Declaration.Variables.Count >= 2)
            {
                UdonSharpBehaviourUtility.ReportDiagnosticsIfValid(context, RuleSet, declaration, targetProperty);
                return;
            }

            var cls = declaration.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            var callbacks = cls.DescendantNodes()
                               .OfType<FieldDeclarationSyntax>()
                               .Where(w => w.AttributeLists.SelectMany(v => v.Attributes).Any(IsFieldChangeCallbackAttribute));

            foreach (var callback in callbacks)
            {
                if (callback.IsEquivalentTo(declaration))
                    continue;

                var attr = callback.AttributeLists.SelectMany(w => w.Attributes).First(IsFieldChangeCallbackAttribute);
                if (attr.ArgumentList.Arguments.FirstOrDefault()?.Expression.ParseValue() == targetProperty)
                {
                    UdonSharpBehaviourUtility.ReportDiagnosticsIfValid(context, RuleSet, declaration, targetProperty);
                    break;
                }
            }
        }
    }
}