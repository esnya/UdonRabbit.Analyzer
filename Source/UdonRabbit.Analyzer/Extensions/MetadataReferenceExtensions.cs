using Microsoft.CodeAnalysis;

namespace UdonRabbit.Analyzer.Extensions
{
    public static class MetadataReferenceExtensions
    {
        public static string ToFilePath(this MetadataReference reference)
        {
            // JetBrains Rider/OmniSharp returns PortableExecutableReference
            if (reference is PortableExecutableReference per)
                return per.FilePath;

            //But Microsoft Visual Studio returns SerializerService+SerializedMetadataReference
            return reference.Display;
        }
    }
}