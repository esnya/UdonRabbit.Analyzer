using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using UnityEditor;

using UnityEngine;

namespace Mochizuki.VRChat.Analyzer
{
    public class SolutionHook : AssetPostprocessor
    {
		private const string Analyzer = "UdonRabbit.Analyzer";
		private const string XmlNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

		private static string OnGeneratedCSProject(string path, string content)
        {
            var version = ReadAnalyzerVersionFromPackages(path);
            if (string.IsNullOrWhiteSpace(version))
                return content;

            var analyzer = GetAnalyzerPathFromCsprojPath(path, version);

            var document = XDocument.Parse(content);
			var @namespace = (XNamespace) XmlNamespace;
			var element = new XElement(@namespace + "ItemGroup", new XElement(@namespace + "Analyzer", new XAttribute("Include", Path.Combine(analyzer, $"{Analyzer}.dll"))));
            document.Root?.Add(element);

            return document.Declaration + Environment.NewLine + document.Root;
        }

        private static string ReadAnalyzerVersionFromPackages(string csproj)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(csproj), "packages.config"));
            if (!File.Exists(path))
                return string.Empty;

            using (var sr = new StreamReader(path))
            {
                var document = XDocument.Parse(sr.ReadToEnd());
                var analyzer = document.Root?.Descendants().FirstOrDefault(w => w.Attribute("id")?.Value == Analyzer);

                return analyzer?.Attribute("version")?.Value;
            }
        }

        private static string GetUnityProjectDirectory()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        }

        private static string GetAnalyzerPathFromCsprojPath(string csproj, string version)
        {
            var path = Path.Combine("Packages", $"{Analyzer}.{version}", "analyzers", "dotnet", "cs");
            return Path.Combine(Path.GetDirectoryName(csproj)?.Replace(GetUnityProjectDirectory(), "") ?? throw new InvalidOperationException(), path);
        }
    }
}