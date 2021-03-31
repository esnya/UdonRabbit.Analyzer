using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Extensions;

namespace UdonRabbit.Analyzer.Udon
{
    public static class UdonAssemblyLoader
    {
        private static readonly object LockObj = new();

        private static readonly HashSet<string> UdonAllowAssemblies = new()
        {
            "VRCSDK3",
            "VRCSDKBase",
            "Cinemachine",
            "Unity.Postprocessing.Runtime",
            "Unity.TextMeshPro"
        };

        public static bool IsAssemblyLoaded { get; private set; }

        public static Assembly UdonAssembly { get; private set; }

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

        public static void LoadUdonAssemblies(List<MetadataReference> references)
        {
            lock (LockObj)
            {
                if (IsAssemblyLoaded)
                    return;

                var path = references.First(w => w.ToFilePath().EndsWith("VRCSDK3.dll"));
                var resolver = new UdonAssemblyResolver(FindAssetsDirectoryFromPath(path.ToFilePath()));

                AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
                {
                    var name = args.Name.Split(',').First();
                    if (name.EndsWith(".resources"))
                        return null;

                    if (references.Any(w => w.ToFilePath().EndsWith($"{name}.dll")))
                        return Assembly.LoadFrom(references.First(w => w.ToFilePath().EndsWith($"{name}.dll")).ToFilePath());
                    return Assembly.LoadFrom(resolver.Resolve($"{name}.dll"));
                };

                Assembly.LoadFrom(resolver.Resolve("VRC.Udon.Editor.dll"));
                foreach (var allowed in UdonAllowAssemblies)
                    Assembly.LoadFrom(resolver.Resolve($"{allowed}.dll"));

                var assembly = AppDomain.CurrentDomain.GetAssemblies().First(w => w.GetName().Name == "VRC.Udon.Editor");
                if (assembly.GetType("VRC.Udon.Editor.UdonEditorManager") == null)
                    return;

                UdonAssembly = assembly;
                IsAssemblyLoaded = true;
            }
        }
    }
}