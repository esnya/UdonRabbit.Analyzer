using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Extensions;

namespace UdonRabbit.Analyzer.Udon
{
    public static class UdonAssemblyVersion
    {
        private const string UdonGuid = "473737f63e15e204aa3a3df7a3a173e3";
        private const string UdonSharpGuid = "cb4c25a39519c854fbe183f6a7ec08f7";
        private const string SDKGuid = "2cdbe2e71e2c46e48951c13df254e5b1";

        public static string UdonVersion { get; private set; }
        public static string UdonSharpVersion { get; private set; }
        public static string SDKVersion { get; private set; }

        private static IEnumerable<string> FindAssetsDirectoryFromPath(string path)
        {
            var paths = new List<string>();

            var lastIndex = 0;
            while (path.IndexOf("Assets", lastIndex, StringComparison.InvariantCulture) >= 0)
            {
                var idx = path.IndexOf("Assets", lastIndex, StringComparison.InvariantCulture);
                paths.Add(path.Substring(0, idx));

                lastIndex = idx + "Assets".Length;
            }

            paths.Reverse();
            return paths;
        }

        public static void Initialize(List<MetadataReference> references)
        {
            static bool HasSpecifiedGuid(string path, string guid)
            {
                using var sr = new StreamReader(path);
                return sr.ReadToEnd().IndexOf(guid, StringComparison.InvariantCulture) >= 0;
            }

            static string ReadVersionFromMeta(string path)
            {
                using var sr = new StreamReader(Path.Combine(Path.GetDirectoryName(path) ?? throw new InvalidOperationException(), Path.GetFileNameWithoutExtension(path)));
                return sr.ReadLine();
            }

            var paths = FindAssetsDirectoryFromPath(references.First(w => w.ToFilePath().EndsWith("VRCSDK3.dll")).ToFilePath());
            foreach (var path in paths)
            {
                var metas = Directory.GetFiles(Path.Combine(path, "Assets"), "version.txt.meta", SearchOption.AllDirectories);
                foreach (var meta in metas)
                {
                    if (HasSpecifiedGuid(meta, UdonGuid))
                        UdonVersion = ReadVersionFromMeta(meta);
                    if (HasSpecifiedGuid(meta, UdonSharpGuid))
                        UdonSharpVersion = ReadVersionFromMeta(meta);
                    if (HasSpecifiedGuid(meta, SDKGuid))
                        SDKVersion = ReadVersionFromMeta(meta);
                }
            }
        }
    }
}