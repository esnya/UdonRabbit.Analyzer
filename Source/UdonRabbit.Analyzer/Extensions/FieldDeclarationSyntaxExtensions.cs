using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UdonRabbit.Analyzer.Extensions
{
    public static class FieldDeclarationSyntaxExtensions
    {
        public static bool HasAttribute(this FieldDeclarationSyntax obj, SemanticModel m, string fullyQualifiedMetadataName)
        {
            var s = m.Compilation.GetTypeByMetadataName(fullyQualifiedMetadataName);
            return obj.AttributeLists.SelectMany(w => w.Attributes).Any(w =>
            {
                var t = m.GetTypeInfo(w);
                return t.Type.Equals(s, SymbolEqualityComparer.Default);
            });
        }

        public static List<AttributeSyntax> GetAttributes(this FieldDeclarationSyntax obj, SemanticModel m, string fullyQualifiedMetadataName)
        {
            var s = m.Compilation.GetTypeByMetadataName(fullyQualifiedMetadataName);
            return obj.AttributeLists.SelectMany(w => w.Attributes).Where(w =>
            {
                var t = m.GetTypeInfo(w);
                return t.Type.Equals(s, SymbolEqualityComparer.Default);
            }).ToList();
        }
    }
}