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
    public class NotSupportVariablesOfType : DiagnosticAnalyzer
    {
        public const string ComponentId = "URA0032";
        private const string Category = UdonConstants.UdonCategory;
        private const string HelpLinkUri = "https://github.com/mika-f/UdonRabbit.Analyzer/blob/master/docs/analyzers/URA0032.md";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.URA0032Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.URA0032MessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.URA0032Description), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor RuleSet = new(ComponentId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description, HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleSet);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeVariableDeclaration, SyntaxKind.VariableDeclaration);
        }

        private static void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (VariableDeclarationSyntax) context.Node;
            if (!UdonSharpBehaviourUtility.ShouldAnalyzeSyntax(context.SemanticModel, declaration))
                return;

            if (!UdonAssemblyLoader.IsAssemblyLoaded)
                UdonAssemblyLoader.LoadUdonAssemblies(context.Compilation.ExternalReferences.ToList());

            if (UdonSymbols.Instance == null)
                UdonSymbols.Initialize();

            var type = declaration.Type;
            var typeSymbol = context.SemanticModel.GetTypeInfo(type);
            if (UdonSymbols.Instance?.FindUdonTypeName(context.SemanticModel, typeSymbol.Type) == false)
                UdonSharpBehaviourUtility.ReportDiagnosticsIfValid(context, RuleSet, declaration, typeSymbol.Type.ToDisplayString());
        }
    }
}