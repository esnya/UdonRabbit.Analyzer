using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using UdonRabbit.Analyzer.Udon;

namespace UdonRabbit.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class OnlySupportsOneTypeGenerics : DiagnosticAnalyzer
    {
        private const string ComponentId = "URA0003";
        private const string Category = UdonConstants.UdonSharpCategory;
        private const string HelpLinkUri = "https://docs.mochizuki.moe/udon-rabbit/packages/analyzer/analyzers/URA0003/";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.URA0003Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.URA0003MessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.URA0003Description), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor RuleSet = new(ComponentId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description, HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleSet);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax) context.Node;
            var classDeclaration = invocation.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclaration == null)
                return;

            var declaration = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
            if (!declaration.BaseType.Equals(context.SemanticModel.Compilation.GetTypeByMetadataName(UdonConstants.UdonSharpBehaviourFullName), SymbolEqualityComparer.Default))
                return;

            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation);
            if (methodSymbol.Symbol is not IMethodSymbol method)
                return;

            if (!method.IsGenericMethod)
                return;

            var generics = method.TypeArguments.Length;
            if (generics > 1)
                context.ReportDiagnostic(Diagnostic.Create(RuleSet, invocation.GetLocation(), generics));
        }
    }
}