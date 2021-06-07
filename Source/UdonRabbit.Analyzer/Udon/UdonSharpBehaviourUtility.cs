using System;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UdonRabbit.Analyzer.Udon
{
    public static class UdonSharpBehaviourUtility
    {
        private static readonly CSharpParseOptions Options;

        static UdonSharpBehaviourUtility()
        {
            Options = CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.None).WithPreprocessorSymbols("COMPILER_UDONSHARP");
        }

        public static bool ShouldAnalyzeSyntax(SemanticModel semanticModel, SyntaxNode node)
        {
            var classDecl = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDecl == null)
                return false;

            var declSymbol = (INamedTypeSymbol) ModelExtensions.GetDeclaredSymbol(semanticModel, classDecl);
            return declSymbol.BaseType.Equals(semanticModel.Compilation.GetTypeByMetadataName(UdonConstants.UdonSharpBehaviourFullName), SymbolEqualityComparer.Default);
        }

        public static void ReportDiagnosticsIfValid(SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor, CSharpSyntaxNode node, params object[] messageArgs)
        {
            if (ShouldReportAnalyzerReport(node))
                context.ReportDiagnostic(Diagnostic.Create(descriptor, node.GetLocation(), messageArgs));
        }

        public static void ReportDiagnosticsIfValid(SemanticModelAnalysisContext context, DiagnosticDescriptor descriptor, CSharpSyntaxNode node, params object[] messageArgs)
        {
            if (ShouldReportAnalyzerReport(node))
                context.ReportDiagnostic(Diagnostic.Create(descriptor, node.GetLocation(), messageArgs));
        }

        private static bool ShouldReportAnalyzerReport(SyntaxNode node)
        {
            var tree = CSharpSyntaxTree.ParseText(node.SyntaxTree.GetText(), Options);
            var matched = tree.GetRoot().FindNode(node.Span);

            // When comparing the Node that inputted with the Node at the same position with COMPILER_UDONSHARP enabled, if the Span positions do not match, it is marked as unreachable code, which can be excluded from the analyze.
            return node.Span.Start == matched.Span.Start && node.Span.End == matched.Span.End;
        }

        public static bool ShouldAnalyzeSyntaxByClass(SemanticModel semanticModel, ClassDeclarationSyntax @class)
        {
            var decl = (INamedTypeSymbol) ModelExtensions.GetDeclaredSymbol(semanticModel, @class);
            return decl.BaseType.Equals(semanticModel.Compilation.GetTypeByMetadataName(UdonConstants.UdonSharpBehaviourFullName), SymbolEqualityComparer.Default);
        }

        public static bool IsUdonSharpDefinedTypes(SemanticModel model, ITypeSymbol symbol)
        {
            return
                symbol.Equals(model.Compilation.GetTypeByMetadataName(UdonConstants.UdonSharpBehaviourFullName), SymbolEqualityComparer.Default) ||
                symbol.Equals(model.Compilation.GetTypeByMetadataName(UdonConstants.UdonSharpBehaviourSyncModeFullName), SymbolEqualityComparer.Default) ||
                symbol.Equals(model.Compilation.GetTypeByMetadataName(UdonConstants.UdonSharpSyncModeFullName), SymbolEqualityComparer.Default);
        }

        public static bool IsUdonSharpDefinedTypes(SemanticModel model, ITypeSymbol symbol, TypeKind kind)
        {
            return kind switch
            {
                TypeKind.Array when symbol is IArrayTypeSymbol a => IsUdonSharpDefinedTypes(model, a.ElementType),
                TypeKind.Class => IsUdonSharpDefinedTypes(model, symbol),
                _ => throw new NotSupportedException(kind.ToString())
            };
        }

        private static bool IsUserDefinedTypesInternal(SemanticModel model, ITypeSymbol symbol)
        {
            if (symbol.BaseType == null)
                return false;
            return symbol.BaseType.Equals(model.Compilation.GetTypeByMetadataName(UdonConstants.UdonSharpBehaviourFullName), SymbolEqualityComparer.Default);
        }

        public static bool IsUserDefinedTypes(SemanticModel model, ITypeSymbol symbol, TypeKind kind)
        {
            return kind switch
            {
                TypeKind.Array when symbol is IArrayTypeSymbol a => IsUserDefinedTypes(model, a.ElementType, a.ElementType.TypeKind),
                TypeKind.Class => IsUserDefinedTypesInternal(model, symbol), // UdonSharp currently support user-defined types only.
                _ => false
            };
        }

        public static bool HasUdonSyncedAttribute(SemanticModel model, SyntaxList<AttributeListSyntax> attributes)
        {
            var attrs = attributes.SelectMany(w => w.Attributes)
                                  .Select(w => ModelExtensions.GetSymbolInfo(model, w))
                                  .Select(w => w.Symbol)
                                  .OfType<IMethodSymbol>();

            return attrs.Any(w => PrettyTypeName(w) == "UdonSharp.UdonSyncedAttribute");
        }

        public static bool IsUdonSyncMode(SemanticModel model, SyntaxList<AttributeListSyntax> attributes, string mode)
        {
            var attr = attributes.SelectMany(w => w.Attributes)
                                 .Select(w => (Attribute: w, SymbolInfo: ModelExtensions.GetSymbolInfo(model, w)))
                                 .Where(w => w.SymbolInfo.Symbol is IMethodSymbol)
                                 .FirstOrDefault(w => PrettyTypeName(w.SymbolInfo.Symbol) == "UdonSharp.UdonSyncedAttribute");

            if (attr.Equals(default))
                return false;

            if (attr.Attribute.ArgumentList == null)
                return mode == "None";

            return attr.Attribute.ArgumentList.Arguments.Select(w => w.Expression)
                       .Any(w =>
                       {
                           var info = ModelExtensions.GetSymbolInfo(model, w);
                           if (info.Symbol is not IFieldSymbol field)
                               return mode == "None";
                           return field.Type.ToDisplayString() == "UdonSharp.UdonSyncMode" && field.Name == mode;
                       });
        }

        public static bool HasUdonBehaviourSyncModeAttribute(SemanticModel model, SyntaxNode node)
        {
            var classDecl = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDecl == null)
                return false;

            var attrs = classDecl.AttributeLists.SelectMany(w => w.Attributes)
                                 .Select(w => ModelExtensions.GetSymbolInfo(model, w))
                                 .Select(w => w.Symbol)
                                 .OfType<IMethodSymbol>();

            return attrs.Any(w => PrettyTypeName(w) == "UdonSharp.UdonBehaviourSyncModeAttribute");
        }

        public static bool IsUdonBehaviourSyncMode(SemanticModel model, SyntaxNode node, string mode)
        {
            var classDecl = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDecl == null)
                return false;

            var attr = classDecl.AttributeLists.SelectMany(w => w.Attributes)
                                .Select(w => (Attribute: w, SymbolInfo: ModelExtensions.GetSymbolInfo(model, w)))
                                .Where(w => w.SymbolInfo.Symbol is IMethodSymbol)
                                .FirstOrDefault(w => PrettyTypeName(w.SymbolInfo.Symbol) == "UdonSharp.UdonBehaviourSyncModeAttribute");

            if (attr.Equals(default))
                return false;

            if (attr.Attribute.ArgumentList == null)
                return mode == "Any";

            return attr.Attribute.ArgumentList.Arguments.Select(w => w.Expression)
                       .Any(w =>
                       {
                           var info = ModelExtensions.GetSymbolInfo(model, w);
                           if (info.Symbol is not IFieldSymbol field)
                               return mode == "Any";
                           return field.Type.ToDisplayString() == "UdonSharp.BehaviourSyncMode" && field.Name == mode;
                       });
        }

        public static string PrettyTypeName(ISymbol symbol)
        {
            return symbol switch
            {
                IArrayTypeSymbol a => $"{a.ToDisplayString()}",
                IMethodSymbol m => $"{m.ContainingType.ToDisplayString()}",
                INamedTypeSymbol t => $"{t.ToDisplayString()}",
                _ => string.Empty
            };
        }
    }
}