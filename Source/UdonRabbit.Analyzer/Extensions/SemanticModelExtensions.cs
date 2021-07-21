using System.Linq;

using Microsoft.CodeAnalysis;

namespace UdonRabbit.Analyzer.Extensions
{
    public static class SemanticModelExtensions
    {
        public static IPropertySymbol LookupProperty(this SemanticModel m, SyntaxNode node, string name)
        {
            return m.LookupSymbols(node.SpanStart, null, name).OfType<IPropertySymbol>().FirstOrDefault();
        }
    }
}