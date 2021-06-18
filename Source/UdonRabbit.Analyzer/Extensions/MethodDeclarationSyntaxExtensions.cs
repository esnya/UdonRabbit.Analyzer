using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using UdonRabbit.Analyzer.Models;

namespace UdonRabbit.Analyzer.Extensions
{
    public static class MethodDeclarationSyntaxExtensions
    {
        public static IEnumerable<SyntaxNodeWithSymbol<InvocationExpressionSyntax>> ScanMethodCallers(this MethodDeclarationSyntax declaration, SemanticModel sm, Func<IMethodSymbol, bool> predicate)
        {
            var @class = declaration.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            return @class.DescendantNodes()
                         .OfType<InvocationExpressionSyntax>()
                         .Select(w => new SyntaxNodeWithSymbol<InvocationExpressionSyntax> { Info = sm.GetSymbolInfo(w), Node = w })
                         .Where(w => w.Info.Symbol is IMethodSymbol symbol && predicate.Invoke(symbol))
                         .ToList();
        }
    }
}