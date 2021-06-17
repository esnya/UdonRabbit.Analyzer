using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace UdonRabbit.Analyzer.Extensions
{
    public static class SemanticModelExtensions
    {
        public static bool CanReferenceNamedSymbol(this SemanticModel obj, TextSpan span, string name)
        {
            return obj.LookupNamespacesAndTypes(span.Start, null, name).Any();
        }

        public static bool ShouldUseQualifiedName(this SemanticModel obj, TextSpan span, string name)
        {
            return obj.LookupNamespacesAndTypes(span.Start, null, name).Length > 1;
        }
    }
}