using System.Collections.Generic;
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
            context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (ClassDeclarationSyntax) context.Node;
            if (!UdonSharpBehaviourUtility.ShouldAnalyzeSyntax(context.SemanticModel, declaration))
                return;

            var classes = new List<ClassDeclarationSyntax>();
            foreach (var unit in context.Compilation.SyntaxTrees.Select(w => w.GetCompilationUnitRoot()))
            {
                void RecursiveFindClassDeclarations(List<MemberDeclarationSyntax> members)
                {
                    classes.AddRange(members.OfType<ClassDeclarationSyntax>());

                    var i = members.OfType<NamespaceDeclarationSyntax>().SelectMany(w => w.Members).ToList();
                    if (i.Any())
                        RecursiveFindClassDeclarations(i);
                }

                RecursiveFindClassDeclarations(unit.Members.ToList());
            }

            if (classes.Count > 1 && classes.Skip(1).Any(w => w == declaration))
                context.ReportDiagnostic(Diagnostic.Create(RuleSet, declaration.GetLocation()));
        }
    }
}