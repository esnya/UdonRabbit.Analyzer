using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

// ReSharper disable StaticMemberInGenericType

namespace UdonRabbit.Analyzer.Test.Infrastructure
{
    public abstract class DiagnosticVerifier<TAnalyzer> where TAnalyzer : DiagnosticAnalyzer, new()
    {
        private static readonly HashSet<string> UnityEngineAssemblies = new()
        {
            "UnityEditor",
            "UnityEngine"
            /*
            "UnityEngine.AIModule",
            "UnityEngine.ARModule",
            "UnityEngine.AccessibilityModule",
            "UnityEngine.AnimationModule",
            "UnityEngine.AssetBundleModule",
            "UnityEngine.AudioModule",
            "UnityEngine.BaselibModule",
            "UnityEngine.ClothModule",
            "UnityEngine.ClusterInputModule",
            "UnityEngine.ClusterRendererModule",
            "UnityEngine.CoreModule",
            "UnityEngine.CrashReportingModule",
            "UnityEngine.DirectorModule",
            "UnityEngine.FileSystemHttpModule",
            "UnityEngine.GameCenterModule",
            "UnityEngine.GridModule",
            "UnityEngine.HotReloadModule",
            "UnityEngine.IMGUIModule",
            "UnityEngine.ImageConversionModule",
            "UnityEngine.InputModule",
            "UnityEngine.JSONSerializeModule",
            "UnityEngine.LocalizationModule",
            "UnityEngine.ParticleSystemModule",
            "UnityEngine.PerformanceReportingModule",
            "UnityEngine.PhysicsModule",
            "UnityEngine.Physics2DModule",
            "UnityEngine.ProfilerModule",
            "UnityEngine.ScreenCaptureModule",
            "UnityEngine.SharedInternalsModule",
            "UnityEngine.SpriteMaskModule",
            "UnityEngine.SpriteShapeModule",
            "UnityEngine.StreamingModule",
            "UnityEngine.StyleSheetsModule",
            "UnityEngine.SubstanceModule",
            "UnityEngine.TLSModule",
            "UnityEngine.TerrainModule",
            "UnityEngine.TerrainPhysicsModule",
            "UnityEngine.TextCoreModule",
            "UnityEngine.TextRenderingModule",
            "UnityEngine.TilemapModule",
            "UnityEngine.TimelineModule",
            "UnityEngine.UIModule",
            "UnityEngine.UIElementsModule",
            "UnityEngine.UNETModule",
            "UnityEngine.UmbraModule",
            "UnityEngine.UnityAnalyticsModule",
            "UnityEngine.UnityConnectModule",
            "UnityEngine.UnityTestProtocolModule",
            "UnityEngine.UnityWebRequestModule",
            "UnityEngine.UnityWebRequestAssetBundleModule",
            "UnityEngine.UnityWebRequestAudioModule",
            "UnityEngine.UnityWebRequestTextureModule",
            "UnityEngine.UnityWebRequestWWWModule",
            "UnityEngine.VFXModule",
            "UnityEngine.VRModule",
            "UnityEngine.VehiclesModule",
            "UnityEngine.VideoModule",
            "UnityEngine.WindModule",
            "UnityEngine.XRModule",
            "Unity.Locator"
            */
        };

        private static readonly HashSet<string> MonoBleedingEdgeAssemblies = new()
        {
            "mscorlib",
            "System"
            /*
            "System.Core",
            "System.Runtime.Serialization",
            "System.Xml",
            "System.Xml.Linq",
            "System.Numerics",
            "System.Numerics.Vectors",
            "System.Net.Http",
            "Microsoft.CSharp",
            "System.Data",
            "Microsoft.Win32.Primitives",
            "System.AppContext",
            "System.Collections.Concurrent",
            "System.Collections",
            "System.Collections.NonGeneric",
            "System.Collections.Specialized",
            "System.ComponentModel.Annotations",
            "System.ComponentModel",
            "System.ComponentModel.EventBasedAsync",
            "System.ComponentModel.Primitives",
            "System.ComponentModel.TypeConverter",
            "System.Console",
            "System.Data.Common",
            "System.Diagnostics.Contracts",
            "System.Diagnostics.Debug",
            "System.Diagnostics.FileVersionInfo",
            "System.Diagnostics.Process",
            "System.Diagnostics.StackTrace",
            "System.Diagnostics.TextWriterTraceListener",
            "System.Diagnostics.Tools",
            "System.Diagnostics.TraceSource",
            "System.Drawing.Primitives",
            "System.Dynamic.Runtime",
            "System.Globalization.Calendars",
            "System.Globalization",
            "System.Globalization.Extensions",
            "System.IO.Compression.ZipFile",
            "System.IO",
            "System.IO.FileSystem",
            "System.IO.FileSystem.DriveInfo",
            "System.IO.FileSystem.Primitives",
            "System.IO.FileSystem.Watcher",
            "System.IO.IsolatedStorage",
            "System.IO.MemoryMappedFiles",
            "System.IO.Pipes",
            "System.IO.UnmanagedMemoryStream",
            "System.Linq",
            "System.Linq.Expressions",
            "System.Linq.Parallel",
            "System.Linq.Queryable",
            "System.Net.Http.Rtc",
            "System.Net.NameResolution",
            "System.Net.NetworkInformation",
            "System.Net.Ping",
            "System.Net.Primitives",
            "System.Net.Requests",
            "System.Net.Security",
            "System.Net.Sockets",
            "System.Net.WebHeaderCollection",
            "System.Net.WebSockets.Client",
            "System.Net.WebSockets",
            "System.ObjectModel",
            "System.Reflection",
            "System.Reflection.Emit",
            "System.Reflection.Emit.ILGeneration",
            "System.Reflection.Emit.Lightweight",
            "System.Reflection.Extensions",
            "System.Reflection.Primitives",
            "System.Resources.Reader",
            "System.Resources.ResourceManager",
            "System.Resources.Writer",
            "System.Runtime.CompilerServices.VisualC",
            "System.Runtime",
            "System.Runtime.Extensions",
            "System.Runtime.Handles",
            "System.Runtime.InteropServices",
            "System.Runtime.InteropServices.RuntimeInformation",
            "System.Runtime.InteropServices.WindowsRuntime",
            "System.Runtime.Numerics",
            "System.Runtime.Serialization.Formatters",
            "System.Runtime.Serialization.Json",
            "System.Runtime.Serialization.Primitives",
            "System.Runtime.Serialization.Xml",
            "System.Security.Claims",
            "System.Security.Cryptography.Algorithms",
            "System.Security.Cryptography.Csp",
            "System.Security.Cryptography.Encoding",
            "System.Security.Cryptography.Primitives",
            "System.Security.Cryptography.X509Certificates",
            "System.Security.Principal",
            "System.Security.SecureString",
            "System.ServiceModel.Duplex",
            "System.ServiceModel.Http",
            "System.ServiceModel.NetTcp",
            "System.ServiceModel.Primitives",
            "System.ServiceModel.Security",
            "System.Text.Encoding",
            "System.Text.Encoding.Extensions",
            "System.Text.RegularExpressions",
            "System.Threading",
            "System.Threading.Overlapped",
            "System.Threading.Tasks",
            "System.Threading.Tasks.Parallel",
            "System.Threading.Thread",
            "System.Threading.ThreadPool",
            "System.Threading.Timer",
            "System.ValueTuple",
            "System.Xml.ReaderWriter",
            "System.Xml.XDocument",
            "System.Xml.XmlDocument",
            "System.Xml.XmlSerializer",
            "System.Xml.XPath",
            "System.Xml.XPath.XDocument"
            */
        };

        private static readonly HashSet<string> UdonAssemblies = new()
        {
            "VRC.Udon.Compiler",
            "VRC.Udon.EditorBindings",
            "VRC.Udon.Graph",
            "VRC.Udon.UAssembly",
            "VRC.Udon.VRCGraphModules",
            "VRC.Udon.VRCTypeResolverModules",
            "VRC.Udon.ClientBindings",
            "VRC.Udon.Common",
            "VRC.Udon.Security",
            "VRC.Udon.VM",
            "VRC.Udon.Wrapper"
        };

        private static readonly HashSet<string> ScriptableAssemblies = new()
        {
            "Cinemachine",
            "UdonSharp.Editor",
            "UdonSharp.Runtime",
            "Unity.Postprocessing.Editor",
            "Unity.Postprocessing.Runtime",
            "Unity.TextMeshPro",
            "VRC.Udon",
            "VRC.Udon.Editor",
            "VRC.Udon.Serialization.OdinSerializer"
        };

        private static readonly HashSet<string> SdkAssemblies = new()
        {
            "System.Buffers",
            "System.Collections.Immutable",
            "System.Memory",
            "System.Numerics.Vectors",
            "System.Runtime.CompilerServices.Unsafe",
            "VRCCore-Editor",
            "VRCSDK3-Editor",
            "VRCSDK3",
            "VRCSDKBase-Editor",
            "VRCSDKBase"
        };

        public static async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
        {
            var testProject = new TestProject
            {
                TestCode = source
            };

            testProject.ExpectedDiagnostics.AddRange(expected);

            await testProject.RunAsync(CancellationToken.None);
        }

        private class TestProject : CSharpAnalyzerTest<TAnalyzer, MSTestVerifier>
        {
            private const string EnvUnity = "UDONRABBIT_ANALYZER_TEST_UNITY";
            private const string EnvMono = "UDONRABBIT_ANALYZER_TEST_MONO";
            private const string EnvUdon = "UDONRABBIT_ANALYZER_TEST_UDON";
            private const string EnvScript = "UDONRABBIT_ANALYZER_TEST_SCRIPTABLE";
            private const string EnvSdk = "UDONRABBIT_ANALYZER_TEST_VRC";

            private static readonly string UnityEditorPath = Environment.GetEnvironmentVariable(EnvUnity);
            private static readonly string MonoPath = Environment.GetEnvironmentVariable(EnvMono);
            private static readonly string UdonPath = Environment.GetEnvironmentVariable(EnvUdon);
            private static readonly string SdkPath = Environment.GetEnvironmentVariable(EnvSdk);
            private static readonly string ScriptablePath = Environment.GetEnvironmentVariable(EnvScript);

            private static HashSet<string> _monoAssembliesPaths;
            private static HashSet<string> _unityAssembliesPaths;
            private static HashSet<string> _udonAssembliesPaths;
            private static HashSet<string> _scriptableAssembliesPaths;
            private static HashSet<string> _sdkAssembliesPaths;

            static TestProject()
            {
                // Workaround for configuring environment variables in xUnit
                using var sr = new StreamReader("./UdonRabbit.runsettings");
                var document = new XPathDocument(sr);
                var navigator = document.CreateNavigator();

                void ConfigureProcessEnvironmentVariableIfNotExists(string env, ref string variable)
                {
                    if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(env)))
                        return;

                    var query = navigator.Compile($"//EnvironmentVariables/{env}");
                    variable = navigator.SelectSingleNode(query)?.Value;
                }

                ConfigureProcessEnvironmentVariableIfNotExists(EnvUnity, ref UnityEditorPath);
                ConfigureProcessEnvironmentVariableIfNotExists(EnvMono, ref MonoPath);
                ConfigureProcessEnvironmentVariableIfNotExists(EnvUdon, ref UdonPath);
                ConfigureProcessEnvironmentVariableIfNotExists(EnvScript, ref ScriptablePath);
                ConfigureProcessEnvironmentVariableIfNotExists(EnvSdk, ref SdkPath);
            }

            public TestProject()
            {
                SolutionTransforms.Add((solution, projectId) =>
                {
                    var compilationOptions = solution.GetProject(projectId)?.CompilationOptions;
                    if (compilationOptions == null)
                        return solution;

                    solution = UnityEngineAssemblies.Aggregate(solution, (sol, dll) => sol.AddMetadataReference(projectId, MetadataReference.CreateFromFile(FindUnityAssemblies(dll))));
                    solution = MonoBleedingEdgeAssemblies.Aggregate(solution, (sol, dll) => sol.AddMetadataReference(projectId, MetadataReference.CreateFromFile(FindMonoAssemblies(dll))));
                    solution = UdonAssemblies.Aggregate(solution, (sol, dll) => sol.AddMetadataReference(projectId, MetadataReference.CreateFromFile(FindUdonAssemblies(dll))));
                    solution = ScriptableAssemblies.Aggregate(solution, (sol, dll) => sol.AddMetadataReference(projectId, MetadataReference.CreateFromFile(FindScriptableAssemblies(dll))));
                    solution = SdkAssemblies.Aggregate(solution, (sol, dll) => sol.AddMetadataReference(projectId, MetadataReference.CreateFromFile(FindSdkAssemblies(dll))));

                    return solution;
                });
            }

            private static string FindUnityAssemblies(string name)
            {
                _unityAssembliesPaths ??= new HashSet<string>(Directory.GetFiles(UnityEditorPath, "*.dll", SearchOption.AllDirectories));
                return _unityAssembliesPaths.First(w => w.EndsWith($"{name}.dll"));
            }

            private static string FindMonoAssemblies(string name)
            {
                _monoAssembliesPaths ??= new HashSet<string>(Directory.GetFiles(MonoPath, "*.dll", SearchOption.AllDirectories));
                return _monoAssembliesPaths.First(w => w.EndsWith($"{name}.dll"));
            }

            private static string FindUdonAssemblies(string name)
            {
                _udonAssembliesPaths ??= new HashSet<string>(Directory.GetFiles(UdonPath, "*.dll", SearchOption.AllDirectories));
                return _udonAssembliesPaths.First(w => w.EndsWith($"{name}.dll"));
            }

            private static string FindScriptableAssemblies(string name)
            {
                _scriptableAssembliesPaths ??= new HashSet<string>(Directory.GetFiles(ScriptablePath, "*.dll", SearchOption.AllDirectories));
                return _scriptableAssembliesPaths.First(w => w.EndsWith($"{name}.dll"));
            }

            private static string FindSdkAssemblies(string name)
            {
                _sdkAssembliesPaths ??= new HashSet<string>(Directory.GetFiles(SdkPath, "*.dll", SearchOption.AllDirectories));
                return _sdkAssembliesPaths.First(w => w.EndsWith($"{name}.dll"));
            }
        }
    }
}