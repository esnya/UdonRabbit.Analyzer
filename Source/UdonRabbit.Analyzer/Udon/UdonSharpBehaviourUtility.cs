using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UdonRabbit.Analyzer.Udon
{
    public static class UdonSharpBehaviourUtility
    {
        public static bool ShouldAnalyzeSyntax(SemanticModel semanticModel, SyntaxNode node)
        {
            var classDecl = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDecl == null)
                return false;

            var declSymbol = (INamedTypeSymbol) semanticModel.GetDeclaredSymbol(classDecl);
            return declSymbol.BaseType.Equals(semanticModel.Compilation.GetTypeByMetadataName(UdonConstants.UdonSharpBehaviourFullName), SymbolEqualityComparer.Default);
        }
    }
}