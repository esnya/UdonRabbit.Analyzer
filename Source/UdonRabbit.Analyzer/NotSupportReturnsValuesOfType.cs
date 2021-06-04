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
    public class NotSupportReturnsValuesOfType : DiagnosticAnalyzer
    {
        public const string ComponentId = "URA0026";
        private const string Category = UdonConstants.UdonCategory;
        private const string HelpLinkUri = "https://github.com/mika-f/UdonRabbit.Analyzer/blob/master/docs/analyzers/URA0026.md";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.URA0026Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.URA0026MessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.URA0026Description), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor RuleSet = new(ComponentId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description, HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleSet);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (MethodDeclarationSyntax) context.Node;
            if (!UdonSharpBehaviourUtility.ShouldAnalyzeSyntax(context.SemanticModel, declaration))
                return;

            if (!UdonAssemblyLoader.IsAssemblyLoaded)
                UdonAssemblyLoader.LoadUdonAssemblies(context.Compilation.ExternalReferences.ToList());

            if (UdonSymbols.Instance == null)
                UdonSymbols.Initialize();

            var @return = declaration.ReturnType;
            var typeSymbol = context.SemanticModel.GetTypeInfo(@return);
            if (UdonSymbols.Instance != null && !UdonSymbols.Instance.FindUdonTypeName(context.SemanticModel, typeSymbol.Type))
                UdonSharpBehaviourUtility.ReportDiagnosticsIfValid(context, RuleSet, declaration, typeSymbol.Type.ToDisplayString());
        }
    }
}