using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Extensions;

namespace UdonRabbit.Analyzer.Udon
{
    public class UdonSymbols
    {
        private static readonly object LockObjForAsmLoad = new();
        private static readonly object LockObjForTypeMap = new();

        private static readonly HashSet<string> AllowClassNameList = new()
        {
            "UdonSharpUdonSyncMode",
            "UdonSharpBehaviourSyncMode",
            "UdonSharpUdonSharpBehaviour"
        };

        private static readonly HashSet<string> AllowMethodNameList = new()
        {
            $"{UdonConstants.UdonSharpBehaviour}.__GetProgramVariable__SystemString__SystemObject",
            $"{UdonConstants.UdonSharpBehaviour}.__SetProgramVariable__SystemString_SystemObject__SystemVoid",
            $"{UdonConstants.UdonSharpBehaviour}.__SendCustomEvent__SystemString__SystemObject",
            $"{UdonConstants.UdonSharpBehaviour}.__SendCustomNetworkEvent__VRCUdonCommonInterfacesNetworkEventTarget_SystemString__SystemVoid",
            $"{UdonConstants.UdonSharpBehaviour}.__SendCustomEventDelayedSeconds__SystemString__SystemSingle_VRCUdonCommonEnumsEVentTiming__SystemVoid",
            $"{UdonConstants.UdonSharpBehaviour}.__SendCustomEventDelayedSeconds__SystemString__SystemInt32_VRCUdonCommonEnumsEVentTiming__SystemVoid",
            $"{UdonConstants.UdonSharpBehaviour}.__VRCInstantiate_UnityEngineGameObject__UnityEngineGameObject",
            $"{UdonConstants.UdonSharpBehaviour}.__RequestSerialization__SystemVoid"

            // Should I add to UdonSharpBehaviour utility methods to allow list?
        };

        private static readonly Dictionary<string, Type> BuiltinTypes = new()
        {
            { "void", typeof(void) },
            { "string", typeof(string) },
            { "int", typeof(int) },
            { "uint", typeof(uint) },
            { "long", typeof(long) },
            { "ulong", typeof(ulong) },
            { "short", typeof(short) },
            { "ushort", typeof(ushort) },
            { "char", typeof(char) },
            { "bool", typeof(bool) },
            { "byte", typeof(byte) },
            { "sbyte", typeof(sbyte) },
            { "float", typeof(float) },
            { "double", typeof(double) },
            { "decimal", typeof(decimal) },
            { "object", typeof(object) }
        };

        #region List of Dynamic Link Assemblies to control Exceptions that occur only in the test environment

        // List of assemblies that could not load on test context, missing file on disk system?
        private static readonly HashSet<string> AllowNotLoadedOnContext = new()
        {
            // MonoBleedingEdge,
            "System.Net.Http.Rtc",
            "System.Runtime.InteropServices.WindowsRuntime",
            "System.ServiceModel.Duplex",
            "System.ServiceModel.Http",
            "System.ServiceModel.NetTcp",
            "System.ServiceModel.Primitives",
            "System.ServiceModel.Security",

            // JetBrains
            "JetBrains.Rider.Unity.Editor.Plugin.Repacked"
        };

        #endregion

        private readonly Dictionary<string, string> _builtinEvents;
        private readonly Dictionary<Type, Type> _inheritedTypeMap;
        private readonly HashSet<string> _nodeDefinitions;
        private readonly Dictionary<ITypeSymbol, Type> _symbolToTypeMappings;

        public static UdonSymbols Instance { get; private set; }

        private UdonSymbols(UdonEditorManager manager)
        {
            _builtinEvents = manager?.GetBuiltinEvents() ?? new Dictionary<string, string>();
            _inheritedTypeMap = manager?.GetVrcInheritedTypeMaps() ?? new Dictionary<Type, Type>();
            _nodeDefinitions = new HashSet<string>(manager?.GetUdonNodeDefinitions() ?? Array.Empty<string>());
            _symbolToTypeMappings = new Dictionary<ITypeSymbol, Type>();
        }

        public static void Initialize(Compilation compilation)
        {
            lock (LockObjForAsmLoad)
            {
                if (Instance != null)
                    return;

                var manager = LoadSdkAssemblies(compilation);
                Instance = new UdonSymbols(manager);
            }
        }

        private static UdonEditorManager LoadSdkAssemblies(Compilation compilation)
        {
            var reference = compilation.ExternalReferences.FirstOrDefault(w => w.Display.Contains("VRC.Udon.Common"));
            if (reference == null)
                return null;

            static string FindUnityAssetsDirectory(MetadataReference r)
            {
                return r.ToFilePath().Substring(0, r.ToFilePath().IndexOf("Assets", StringComparison.InvariantCulture));
            }

            var assemblies = Path.GetFullPath(Path.Combine(FindUnityAssetsDirectory(reference), "Library", "ScriptAssemblies"));
            var editor = Path.Combine(assemblies, "VRC.Udon.Editor.dll");
            if (!File.Exists(editor))
                return null; // Invalid Path;

            var loadedAssemblies = new HashSet<string>();

            Assembly ResolveDynamicLoadingAssemblies(object _, ResolveEventArgs args)
            {
                string LoadAssembly(string asmPath)
                {
                    if (compilation.ExternalReferences.Any(w => w.ToFilePath().Contains(asmPath)))
                        return compilation.ExternalReferences.First(w => w.ToFilePath().Contains(asmPath)).ToFilePath();
                    return Path.GetFullPath(Path.Combine(assemblies, asmPath));
                }

                var name = args.Name.Split(',').First();
                if (name.EndsWith(".resources"))
                    return null; // no needed

                var path = LoadAssembly($"{name}.dll");
                if (!File.Exists(path))
                    return null; // could not load

                try
                {
                    var asm = Assembly.LoadFrom(path);

                    loadedAssemblies.Add(path);
                    return asm;
                }
                catch (Exception)
                {
                    if (AllowNotLoadedOnContext.Contains(name))
                    {
                        loadedAssemblies.Add(path);
                        return null;
                    }

                    throw;
                }
            }

            AppDomain.CurrentDomain.AssemblyResolve += ResolveDynamicLoadingAssemblies;
            var assembly = Assembly.LoadFrom(editor);

            foreach (var missingReference in compilation.ExternalReferences.Select(w => w.ToFilePath()))
            {
                if (loadedAssemblies.Contains(missingReference))
                    continue;

                if (!File.Exists(missingReference))
                    continue;

                try
                {
                    AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(missingReference));
                    loadedAssemblies.Add(missingReference);
                }
                catch (Exception)
                {
                    var name = Path.GetFileNameWithoutExtension(missingReference);
                    if (AllowNotLoadedOnContext.Contains(name))
                        continue;

                    throw;
                }
            }

            var manager = new UdonEditorManager(assembly);
            return manager.HasInstance ? manager : null;
        }

        public bool FindUdonMethodName(SemanticModel model, IMethodSymbol symbol)
        {
            var receiver = symbol.ReceiverType;
            if (receiver.BaseType.Equals(model.Compilation.GetTypeByMetadataName(UdonConstants.UdonSharpBehaviourFullName), SymbolEqualityComparer.Default))
                return true; // User-Defined Method, Skip

            var functionNamespace = SanitizeTypeName($"{receiver.ContainingNamespace.ToDisplayString()}{receiver.Name}").Replace(UdonConstants.UdonBehaviour, UdonConstants.UdonCommonInterfacesReceiver);
            var functionName = $"__{symbol.Name.Trim('_').TrimStart('.')}";

            if (functionName == "__VRCInstantiate")
            {
                functionNamespace = "VRCInstantiate";
                functionName = "__Instantiate";
            }

            var paramsSb = new StringBuilder();
            var parameters = symbol.Parameters;

            if (symbol.MethodKind == MethodKind.Constructor)
            {
                paramsSb.Append("__");
            }
            else if (parameters.Length > 0)
            {
                paramsSb.Append("_");
                foreach (var parameter in parameters)
                    paramsSb.Append($"_{GetUdonNamedType(parameter.Type, parameter.RefKind, true)}");
            }

            var returnsSb = new StringBuilder();

            if (symbol.MethodKind == MethodKind.Constructor)
                returnsSb.Append($"__{GetUdonNamedType(receiver)}");
            else
                returnsSb.Append($"__{GetUdonNamedType(symbol.ReturnType, true)}");

            var signature = $"{functionNamespace}.{functionName}{paramsSb}{returnsSb}";
            return AllowMethodNameList.Contains(signature) || _nodeDefinitions.Contains(signature);
        }

        public bool FindUdonVariableName(SemanticModel model, ITypeSymbol typeSymbol, IFieldSymbol fieldSymbol, bool isSetter)
        {
            if (typeSymbol.BaseType.Equals(model.Compilation.GetTypeByMetadataName(UdonConstants.UdonSharpBehaviourFullName), SymbolEqualityComparer.Default))
                return true; // User-Defined Method, Skip

            var functionNamespace = SanitizeTypeName($"{typeSymbol.ContainingNamespace.ToDisplayString()}{typeSymbol.Name}").Replace(UdonConstants.UdonBehaviour, UdonConstants.UdonCommonInterfacesReceiver);
            if (AllowClassNameList.Contains(functionNamespace))
                return true;

            // WORKAROUND for Enum Accessors
            if (typeSymbol.TypeKind == TypeKind.Enum)
                return _nodeDefinitions.Contains($"Type_{functionNamespace}");

            var functionName = $"__{(isSetter ? "set" : "get")}_{fieldSymbol.Name.Trim('_')}";
            var param = $"__{GetUdonNamedType(fieldSymbol.Type)}";
            var signature = $"{functionNamespace}.{functionName}{param}";
            return _nodeDefinitions.Contains(signature);
        }

        public bool FindUdonVariableName(SemanticModel model, ITypeSymbol typeSymbol, IPropertySymbol symbol, bool isSetter)
        {
            if (typeSymbol.BaseType.Equals(model.Compilation.GetTypeByMetadataName("UdonSharp.UdonSharpBehaviour"), SymbolEqualityComparer.Default))
                return true; // User-Defined Method, Skip

            var functionNamespace = SanitizeTypeName($"{typeSymbol.ContainingNamespace.ToDisplayString()}{typeSymbol.Name}").Replace(UdonConstants.UdonBehaviour, UdonConstants.UdonCommonInterfacesReceiver);
            if (AllowClassNameList.Contains(functionNamespace))
                return true;

            var functionName = $"__{(isSetter ? "set" : "get")}_{symbol.Name.Trim('_')}";
            var param = $"__{GetUdonNamedType(symbol.Type)}";
            if (isSetter)
                param += "__SystemVoid";

            var signature = $"{functionNamespace}.{functionName}{param}";
            return _nodeDefinitions.Contains(signature);
        }

        private Type RemapVrcBaseTypes(Type t)
        {
            var depth = 0;

            while (t!.IsArray)
            {
                t = t.GetElementType();
                depth++;
            }

            if (!_inheritedTypeMap.ContainsKey(t))
                return t;

            t = _inheritedTypeMap[t];
            while (depth-- > 0)
                t = t.MakeArrayType();

            return t;
        }

        private string GetUdonNamedType(ITypeSymbol symbol, RefKind reference, bool isSkipBaseTypeRemap = false)
        {
            var t = ConvertTypeSymbolToType(symbol);
            if (t == null)
                return string.Empty;

            if (reference == RefKind.None)
                return GetUdonNamedType(t, isSkipBaseTypeRemap);
            return GetUdonNamedType(t.MakeByRefType(), isSkipBaseTypeRemap);
        }

        private string GetUdonNamedType(ITypeSymbol symbol, bool isSkipBaseTypeRemap = false)
        {
            var t = ConvertTypeSymbolToType(symbol);
            if (t == null)
                return string.Empty;

            return GetUdonNamedType(t, isSkipBaseTypeRemap);
        }

        // ref: https://github.com/MerlinVR/UdonSharp/blob/v0.19.7/Assets/UdonSharp/Editor/UdonSharpResolverContext.cs#L347-L399
        private string GetUdonNamedType(Type t, bool isSkipBaseTypeRemap = false)
        {
            var e = isSkipBaseTypeRemap ? t : RemapVrcBaseTypes(t);
            var externTypeName = e.GetNameWithoutGenericArity();

            while (e!.IsArray || e!.IsByRef)
                e = e.GetElementType();

            var @namespace = t.Namespace;
            if (e.DeclaringType != null)
            {
                var declaringTypeNamespace = "";
                var declaringType = e.DeclaringType;

                while (declaringType != null)
                {
                    declaringTypeNamespace = $"{e.DeclaringType.Name}.{declaringTypeNamespace}";
                    declaringType = declaringType.DeclaringType;
                }

                @namespace += $".{declaringTypeNamespace}";
            }

            if (externTypeName == "T" || externTypeName == "T[]")
                @namespace = "";

            var signature = SanitizeTypeName($"{@namespace}.{externTypeName}");
            foreach (var g in e.GetGenericArguments())
                signature += GetUdonNamedType(g);

            if (signature == "System.Collection.Generic.ListT".Replace(".", ""))
                signature = "ListT";
            else if (signature == "System.Collection.Generic.IEnumerableT".Replace(".", ""))
                signature = "IEnumerableT";

            return signature.Replace("VRCUdonUdonBehaviour", "VRCUdonCommonInterfacesIUdonEventReceiver");
        }

        private static string SanitizeTypeName(string name)
        {
            return name.Replace(",", "")
                       .Replace(".", "")
                       .Replace("[]", "Array")
                       .Replace("&", "Ref")
                       .Replace("+", "");
        }

        private Type ConvertTypeSymbolToType(ITypeSymbol s)
        {
            lock (LockObjForTypeMap)
            {
                if (_symbolToTypeMappings.ContainsKey(s))
                    return _symbolToTypeMappings[s];

                if (BuiltinTypes.ContainsKey(s.ToDisplayString()))
                    return BuiltinTypes[s.ToDisplayString()];

                static IEnumerable<Type> LoadExportedTypes(Assembly asm)
                {
                    try
                    {
                        return asm.GetTypes();
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        return e.Types;
                    }
                }

                if (_symbolToTypeMappings.ContainsKey(s))
                    return _symbolToTypeMappings[s];

                var t = AppDomain.CurrentDomain.GetAssemblies()
                                 .SelectMany(LoadExportedTypes)
                                 .Where(w => !string.IsNullOrWhiteSpace(w?.FullName))
                                 .FirstOrDefault(w => w.FullName == s.ToDisplayString());
                if (t == null)
                    return null;

                _symbolToTypeMappings.Add(s, t);
                return t;
            }
        }
    }
}