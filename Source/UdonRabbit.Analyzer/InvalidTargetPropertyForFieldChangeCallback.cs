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
    public class InvalidTargetPropertyForFieldChangeCallback : DiagnosticAnalyzer
    {
        public const string ComponentId = "URA0053";
        private const string Category = UdonConstants.UdonSharpCategory;
        private const string HelpLinkUri = "https://github.com/esnya/UdonRabbit.Analyzer/blob/master/docs/analyzers/URA0053.md";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.URA0053Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.URA0053MessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.URA0053Description), Resources.ResourceManager, typeof(Resources));
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

            var attributes = declaration.AttributeLists.SelectMany(w => w.Attributes)
                                        .Select(w => (Attribute: w, Symbol: context.SemanticModel.GetSymbolInfo(w)))
                                        .Select(w => (w.Attribute, w.Symbol.Symbol))
                                        .Where(w => w.Symbol is IMethodSymbol)
                                        .ToList();
            if (attributes.None(w => UdonSharpBehaviourUtility.PrettyTypeName(w.Symbol) == UdonConstants.UdonSharpFieldChangeCallbackFullName))
                return;

            var attribute = attributes.First(w => UdonSharpBehaviourUtility.PrettyTypeName(w.Symbol) == UdonConstants.UdonSharpFieldChangeCallbackFullName).Attribute;
            var argument = attribute.ArgumentList.Arguments.FirstOrDefault();
            if (argument == null)
                return;

            var targetProperty = argument.Expression.ParseValue();
            if (string.IsNullOrWhiteSpace(targetProperty))
                return;

            var property = context.SemanticModel.LookupSymbols(declaration.SpanStart).Where(w => w is IPropertySymbol).FirstOrDefault(w => w.Name == targetProperty);
            if (property == null)
            {
                var cls = declaration.FirstAncestorOrSelf<ClassDeclarationSyntax>();
                var sym = context.SemanticModel.GetDeclaredSymbol(cls);
                UdonSharpBehaviourUtility.ReportDiagnosticsIfValid(context, RuleSet, declaration, sym.Name);
            }
        }
    }
}