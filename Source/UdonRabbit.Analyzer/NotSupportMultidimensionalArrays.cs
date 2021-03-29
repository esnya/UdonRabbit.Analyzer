using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using UdonRabbit.Analyzer.Udon;

namespace UdonRabbit.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NotSupportMultidimensionalArrays : DiagnosticAnalyzer
    {
        public const string ComponentId = "URA0013";
        private const string Category = UdonConstants.UdonSharpCategory;
        private const string HelpLinkUri = "https://github.com/esnya/UdonRabbit.Analyzer/blob/master/docs/analyzers/URA0013.md";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.URA0013Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.URA0013MessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.URA0013Description), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor RuleSet = new(ComponentId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description, HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleSet);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeVariableDeclaration, SyntaxKind.VariableDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeParameter, SyntaxKind.Parameter);
        }

        private static void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (VariableDeclarationSyntax) context.Node;
            if (!UdonSharpBehaviourUtility.ShouldAnalyzeSyntax(context.SemanticModel, declaration))
                return;

            foreach (var variable in declaration.Variables)
                switch (context.SemanticModel.GetDeclaredSymbol(variable))
                {
                    case IFieldSymbol f when IsDimensionalArrayType(f.Type):
                    case ILocalSymbol l when IsDimensionalArrayType(l.Type):
                    case IParameterSymbol m when IsDimensionalArrayType(m.Type):
                        context.ReportDiagnostic(Diagnostic.Create(RuleSet, variable.GetLocation()));
                        break;
                }
        }

        private static void AnalyzeParameter(SyntaxNodeAnalysisContext context)
        {
            var parameter = (ParameterSyntax) context.Node;
            if (!UdonSharpBehaviourUtility.ShouldAnalyzeSyntax(context.SemanticModel, parameter))
                return;

            var s = context.SemanticModel.GetDeclaredSymbol(parameter);
            if (IsDimensionalArrayType(s.Type))
                context.ReportDiagnostic(Diagnostic.Create(RuleSet, parameter.GetLocation()));
        }

        private static bool IsDimensionalArrayType(ITypeSymbol s)
        {
            if (s.TypeKind != TypeKind.Array || s is not IArrayTypeSymbol t)
                return false;

            return t.Rank >= 2;
        }
    }
}