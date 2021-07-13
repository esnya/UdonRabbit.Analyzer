using Microsoft.CodeAnalysis;

namespace UdonRabbit.Analyzer.Models
{
    public class SyntaxNodeWithSymbol<T> where T : SyntaxNode
    {
        public SymbolInfo Info { get; init; }

        public T Node { get; init; }
    }
}