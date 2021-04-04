using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;

using UnityEditor;

using UnityEngine;

namespace Mochizuki.VRChat.Analyzer
{
    public class SolutionGeneratedHook : AssetPostprocessor
    {
        private const string Analyzer = "UdonRabbit.Analyzer";
        private const string AnalyzerNamespace = "moe.mochizuki.vrchat.analyzer";
        private const string XmlNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        private static string OnGeneratedCSProject(string path, string content)
        {
            var analyzerRoot = GetRelativePathForPackages();

            var document = XDocument.Parse(content);
            var @namespace = (XNamespace) XmlNamespace;

            var itemGroup = new XElement(@namespace + "ItemGroup");
            var analyzer = new XElement(@namespace + "Analyzer", new XAttribute("Include", Path.Combine(analyzerRoot, "Plugins", "UdonRabbit.Analyzer.dll")));
            var codeFix = new XElement(@namespace + "Analyzer", new XAttribute("Include", Path.Combine(analyzerRoot, "Plugins", "UdonRabbit.CodeFix.dll")));
            
            itemGroup.Add(analyzer);
            itemGroup.Add(codeFix);
            document.Root?.Add(itemGroup);

            return document.Declaration + Environment.NewLine + document.Root;
        }

        private static string GetRelativePathForPackages([CallerFilePath] string path = "")
        {
            var root = Path.GetFullPath(Application.dataPath);
            var analyzer = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), ".."));
            return (new Uri(root)).MakeRelativeUri(new Uri(analyzer)).ToString();
        }
    }
}
