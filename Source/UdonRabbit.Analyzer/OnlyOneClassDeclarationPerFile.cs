using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using UdonRabbit.Analyzer.Udon;

namespace UdonRabbit.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class OnlyOneClassDeclarationPerFile : DiagnosticAnalyzer
    {
        public const string ComponentId = "URA0023";
        private const string Category = UdonConstants.UdonSharpCategory;
        private const string HelpLinkUri = "https://github.com/esnya/UdonRabbit.Analyzer/blob/master/docs/analyzers/URA0023.md";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.URA0023Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.URA0023MessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.URA0023Description), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor RuleSet = new(ComponentId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description, HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleSet);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSemanticModelAction(AnalyzeSemanticModel);
        }

        private static void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
        {
            var root = context.SemanticModel.SyntaxTree.GetRoot();

            var classes = root.DescendantNodes()
                              .Where(w => w is MemberDeclarationSyntax)
                              .OfType<ClassDeclarationSyntax>()
                              .ToList();

            if (classes.Count <= 1)
                return;

            if (classes.All(w => !UdonSharpBehaviourUtility.ShouldAnalyzeSyntaxByClass(context.SemanticModel, w)))
                return;

            foreach (var @class in classes.Skip(1))
                UdonSharpBehaviourUtility.ReportDiagnosticsIfValid(context, RuleSet, @class);
        }
    }
}