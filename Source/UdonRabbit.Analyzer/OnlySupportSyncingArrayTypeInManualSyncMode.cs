using System.Collections.Immutable;
using System.Diagnostics;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using UdonRabbit.Analyzer.Udon;

namespace UdonRabbit.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class OnlySupportSyncingArrayTypeInManualSyncMode : DiagnosticAnalyzer
    {
        public const string ComponentId = "URA0036";
        private const string Category = UdonConstants.UdonCategory;
        private const string HelpLinkUri = "https://github.com/mika-f/UdonRabbit.Analyzer/blob/master/docs/analyzers/URA0036.md";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.URA0036Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.URA0036MessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.URA0036Description), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor RuleSet = new(ComponentId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description, HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleSet);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);

            Debugger.Launch();
        }

        private static void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (FieldDeclarationSyntax) context.Node;
            if (!UdonSharpBehaviourUtility.ShouldAnalyzeSyntax(context.SemanticModel, declaration))
                return;

            if (!UdonSharpBehaviourUtility.HasUdonBehaviourSyncModeAttribute(context.SemanticModel, declaration))
                return;

            if (!UdonSharpBehaviourUtility.IsUdonBehaviourSyncMode(context.SemanticModel, declaration, "Continuous"))
                return;

            if (!UdonSharpBehaviourUtility.HasUdonSyncedAttribute(context.SemanticModel, declaration.AttributeLists))
                return;

            var info = context.SemanticModel.GetSymbolInfo(declaration.Declaration.Type);
            if (info.Symbol is not IArrayTypeSymbol symbol)
                return;

            context.ReportDiagnostic(Diagnostic.Create(RuleSet, declaration.GetLocation(), UdonSharpBehaviourUtility.PrettyTypeName(symbol)));
        }
    }
}