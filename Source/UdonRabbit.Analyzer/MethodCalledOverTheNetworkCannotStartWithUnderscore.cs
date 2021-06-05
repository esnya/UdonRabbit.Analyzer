using System.Collections.Generic;
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
    public class MethodCalledOverTheNetworkCannotStartWithUnderscore : DiagnosticAnalyzer
    {
        public const string ComponentId = "URA0043";
        private const string Category = UdonConstants.UdonCategory;
        private const string HelpLinkUri = "https://github.com/mika-f/UdonRabbit.Analyzer/blob/master/docs/analyzers/URA0043.md";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.URA0043Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.URA0043MessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.URA0043Description), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor RuleSet = new(ComponentId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);

        private static readonly HashSet<(string, int)> ScannedMethodLists = new()
        {
            ("SendCustomNetworkEvent", 1)
        };

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

            if (!declaration.Identifier.ValueText.StartsWith("_"))
                return;

            var @class = declaration.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            var invocations = @class.DescendantNodes()
                                    .OfType<InvocationExpressionSyntax>()
                                    .Select(w => (Info: ModelExtensions.GetSymbolInfo(context.SemanticModel, w), Syntax: w))
                                    .Where(w =>
                                    {
                                        if (w.Info.Symbol is not IMethodSymbol symbol)
                                            return false;
                                        return ScannedMethodLists.Any(v => v.Item1 == symbol.Name);
                                    })
                                    .ToList();

            var hasCaller = invocations.Any(w =>
            {
                var i = ScannedMethodLists.First(v => v.Item1 == w.Info.Symbol.Name).Item2;
                var arg = w.Syntax.ArgumentList.Arguments.ElementAtOrDefault(i);
                if (arg == null)
                    return false;

                return arg.Expression.ParseValue() == declaration.Identifier.Text;
            });

            if (hasCaller)
                UdonSharpBehaviourUtility.ReportDiagnosticsIfValid(context, RuleSet, declaration);
        }
    }
}