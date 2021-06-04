# Getting Started with UdonRabbit.Analyzer in JetBrains Rider

Install UdonRabbit.Analyzer to JetBrains Rider is the following steps:

1. Add the new NuGet feeds to `NuGet.config`
   1. `Tools` > `NuGet` > `Manage NuGet Packages for Solution`
   2. Open `Sources` tab
   3. Add a new feed
      - Name: `Natsuneko@GitHub`
      - URL: `https://nuget.pkg.github.com/mika-f/index.json`
      - User: Your GitHub ID
      - Password: Your GitHub Personal Access Token (PAT)
      - Enabled: ✓
2. Open `Packages` tab
3. Search `UdonRabbit.Analyzer`
4. Install UdonRabbit.Analyzer to all projects
   - Click this
   - <img src="https://user-images.githubusercontent.com/10832834/112909309-1f9e7900-912c-11eb-8709-a69aa591e595.PNG" height="150px" />
5. Add `OnGeneratedCSProject` hook to your project (see below example)
6. Happy Coding!

## Example of `SolutionHook.cs`

```csharp
// This code is licensed under the Public Domain
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
            var path = Path.GetFullPath(Path.Combine(csproj, "..", "packages.config"));

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
```
