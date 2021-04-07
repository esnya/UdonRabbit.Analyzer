using System.Text;

using Microsoft.CodeAnalysis;

namespace UdonRabbit.Analyzer.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class ITypeSymbolExtensions
    {
        public static string ToClassString(this ITypeSymbol obj)
        {
            if (obj is not INamedTypeSymbol sym)
                return obj.ToDisplayString();

            var arguments = sym.TypeArguments.Length;
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(sym.ContainingSymbol?.Name))
                sb.Append($"{sym.ContainingSymbol.ToDisplayString()}.");
            sb.Append(sym.Name);
            if (arguments > 0)
                sb.Append($"`{arguments}");

            return sb.ToString();
        }
    }
}