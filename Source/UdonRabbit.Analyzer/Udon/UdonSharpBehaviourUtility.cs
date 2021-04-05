using System;
using System.Linq;

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

        public static bool ShouldAnalyzeSyntaxByClass(SemanticModel semanticModel, ClassDeclarationSyntax @class)
        {
            var decl = (INamedTypeSymbol) semanticModel.GetDeclaredSymbol(@class);
            return decl.BaseType.Equals(semanticModel.Compilation.GetTypeByMetadataName(UdonConstants.UdonSharpBehaviourFullName), SymbolEqualityComparer.Default);
        }

        public static bool IsUserDefinedTypes(SemanticModel model, ITypeSymbol symbol)
        {
            return symbol.BaseType.Equals(model.Compilation.GetTypeByMetadataName(UdonConstants.UdonSharpBehaviourFullName), SymbolEqualityComparer.Default);
        }

        public static bool IsUserDefinedTypes(SemanticModel model, ITypeSymbol symbol, TypeKind kind)
        {
            return kind switch
            {
                TypeKind.Array when symbol is IArrayTypeSymbol a => IsUserDefinedTypes(model, a.ElementType),
                TypeKind.Class => IsUserDefinedTypes(model, symbol),
                _ => throw new NotSupportedException(kind.ToString())
            };
        }

        public static bool HasUdonSyncedAttribute(SemanticModel model, SyntaxList<AttributeListSyntax> attributes)
        {
            var attrs = attributes.SelectMany(w => w.Attributes)
                                  .Select(w => model.GetSymbolInfo(w))
                                  .Select(w => w.Symbol)
                                  .OfType<IMethodSymbol>();

            return attrs.Any(w => PrettyTypeName(w) == "UdonSharp.UdonSyncedAttribute");
        }

        public static bool IsUdonSyncMode(SemanticModel model, SyntaxList<AttributeListSyntax> attributes, string mode)
        {
            var attr = attributes.SelectMany(w => w.Attributes)
                                 .Select(w => (Attribute: w, SymbolInfo: model.GetSymbolInfo(w)))
                                 .Where(w => w.SymbolInfo.Symbol is IMethodSymbol)
                                 .FirstOrDefault(w => PrettyTypeName(w.SymbolInfo.Symbol) == "UdonSharp.UdonSyncedAttribute");

            if (attr.Equals(default))
                return false;

            return attr.Attribute.ArgumentList.Arguments.Select(w => w.Expression)
                       .Any(w =>
                       {
                           var info = model.GetSymbolInfo(w);
                           if (info.Symbol is not IFieldSymbol field)
                               return mode == "None";
                           return field.Type.ToDisplayString() == "UdonSharp.UdonSyncMode" && field.Name == mode;
                       });
        }

        public static string PrettyTypeName(ISymbol symbol)
        {
            return symbol switch
            {
                IMethodSymbol m => $"{m.ContainingType.ToDisplayString()}",
                INamedTypeSymbol t => $"{t.ToDisplayString()}",
                _ => string.Empty
            };
        }
    }
}