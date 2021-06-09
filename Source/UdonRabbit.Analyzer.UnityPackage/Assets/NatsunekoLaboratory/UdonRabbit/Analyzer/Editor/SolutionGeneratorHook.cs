using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using UnityEditor;

namespace NatsunekoLaboratory.UdonRabbit.Analyzer
{
    public class SolutionGeneratorHook : AssetPostprocessor
    {
        private static readonly List<string> BlockProjectList = new List<string>
        {
            "UdonSharp.Editor.csproj",
            "UdonSharp.Runtime.csproj",
            "VRC.SDKBase.Editor.BuildPipeline.csproj",
            "VRC.SDKBase.Editor.ShaderStripping.csproj",
            "VRC.Udon.csproj",
            "VRC.Udon.Editor.csproj",
            "VRC.Udon.Editor.UPMImporter.csproj",
            "VRC.Udon.Serialization.OdinSerializer.csproj"
        };

        private static string OnGeneratedCSProject(string path, string content)
        {
            if (BlockProjectList.Any(path.EndsWith))
                return content;

            var analyzers = AssetDatabase.FindAssets("l:RoslynAnalyzer").Select(AssetDatabase.GUIDToAssetPath).ToArray();

            var document = XDocument.Parse(content);
            var @namespace = (XNamespace) "http://schemas.microsoft.com/developer/msbuild/2003";
            var project = document.Element(@namespace + "Project");
            var itemGroup = new XElement(@namespace + "ItemGroup");

            foreach (var analyzer in analyzers)
                if (analyzer.EndsWith("UdonRabbit.Analyzer.dll") || analyzer.EndsWith("UdonRabbit.Analyzer.CodeFixes.dll"))
                {
                    var include = new XAttribute("Include", analyzer);
                    var analyzerReference = new XElement(@namespace + "Analyzer", include);

                    itemGroup.Add(analyzerReference);
                }

            project?.Add(itemGroup);

            return document.ToString();
        }
    }
}