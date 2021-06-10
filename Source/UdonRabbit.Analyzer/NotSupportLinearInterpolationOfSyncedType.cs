using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using UdonRabbit.Analyzer.Udon;

namespace UdonRabbit.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NotSupportLinearInterpolationOfSyncedType : DiagnosticAnalyzer
    {
        public const string ComponentId = "URA0034";
        private const string Category = UdonConstants.UdonCategory;
        private const string HelpLinkUri = "https://github.com/mika-f/UdonRabbit.Analyzer/blob/master/docs/analyzers/URA0034.md";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.URA0034Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.URA0034MessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.URA0034Description), Resources.ResourceManager, typeof(Resources));
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

            if (!UdonSharpBehaviourUtility.HasUdonSyncedAttribute(context.SemanticModel, declaration.AttributeLists))
                return;

            if (!UdonSharpBehaviourUtility.IsUdonSyncMode(context.SemanticModel, declaration.AttributeLists, "Linear"))
                return;

            if (!UdonAssemblyLoader.IsAssemblyLoaded)
                UdonAssemblyLoader.LoadUdonAssemblies(context.Compilation.ExternalReferences.ToList());

            if (UdonSymbols.Instance == null)
                UdonSymbols.Initialize();

            var info = context.SemanticModel.GetSymbolInfo(declaration.Declaration.Type);
            if (UdonSymbols.Instance?.FindUdonSyncType((INamedTypeSymbol)info.Symbol, "Linear") == false)
                UdonSharpBehaviourUtility.ReportDiagnosticsIfValid(context, RuleSet, declaration, UdonSharpBehaviourUtility.PrettyTypeName(info.Symbol));
        }
    }
}