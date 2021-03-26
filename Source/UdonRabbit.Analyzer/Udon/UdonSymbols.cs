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
        private readonly Dictionary<string, string> _builtinEvents;

        private readonly Dictionary<string, Type> _builtinTypes = new()
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

        private readonly Dictionary<Type, Type> _inheritedTypeMap;
        private readonly HashSet<string> _nodeDefinitions;
        private readonly Dictionary<ITypeSymbol, Type> _symbolToTypeMappings;

        public static UdonSymbols Instance { get; private set; }

        private UdonSymbols(UdonEditorManager manager)
        {
            _nodeDefinitions = new HashSet<string>(manager?.GetUdonNodeDefinitions() ?? Array.Empty<string>());
            _builtinEvents = manager?.GetBuiltinEvents() ?? new Dictionary<string, string>();
            _inheritedTypeMap = manager?.GetVrcInheritedTypeMaps() ?? new Dictionary<Type, Type>();
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
            var reference = compilation.ExternalReferences.FirstOrDefault(w => w.Display.Contains("VRC.Udon.Common.dll"));
            if (reference == null)
                return null;

            static string FindUnityAssetsDirectory(string path)
            {
                return path.Substring(0, path.IndexOf("Assets", StringComparison.InvariantCulture));
            }

            var assemblies = Path.GetFullPath(Path.Combine(FindUnityAssetsDirectory(reference.Display), "Library", "ScriptAssemblies"));
            var editor = Path.Combine(assemblies, "VRC.Udon.Editor.dll");
            if (!File.Exists(editor))
                return null; // Invalid Path;

            Assembly ResolveDynamicLoadingAssemblies(object _, ResolveEventArgs args)
            {
                var dll = $"{args.Name.Split(',').First()}.dll";
                if (compilation.ExternalReferences.Any(w => w.Display.Contains(dll)))
                    return Assembly.LoadFrom(compilation.ExternalReferences.First(w => w.Display.Contains(dll)).Display);
                return Assembly.LoadFrom(Path.GetFullPath(Path.Combine(assemblies, dll)));
            }

            AppDomain.CurrentDomain.AssemblyResolve += ResolveDynamicLoadingAssemblies;
            var assembly = Assembly.LoadFrom(editor);

            var manager = new UdonEditorManager(assembly);
            return manager.HasInstance ? manager : null;
        }

        public bool FindUdonMethodName(SemanticModel model, IMethodSymbol symbol)
        {
            var receiver = symbol.ReceiverType;
            if (receiver.BaseType.Equals(model.Compilation.GetTypeByMetadataName("UdonSharp.UdonSharpBehaviour"), SymbolEqualityComparer.Default))
                return true; // User-Defined Method, Skip

            var functionNamespace = SanitizeTypeName($"{receiver.ContainingNamespace.Name}{receiver.Name}");
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
                    paramsSb.Append($"_{GetUdonNamedType(parameter.Type, true)}");
            }

            var returnsSb = new StringBuilder();

            if (symbol.MethodKind == MethodKind.Constructor)
                returnsSb.Append($"__{GetUdonNamedType(receiver)}");
            else
                returnsSb.Append($"__{GetUdonNamedType(symbol.ReturnType, true)}");

            var signature = $"{functionNamespace}.{functionName}{paramsSb}{returnsSb}";
            return _nodeDefinitions.Contains(signature);
        }

        public bool FindUdonVariableName(IFieldSymbol symbol)
        {
            return false;
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

            var externTypeName = e.GetNameWithoutGenericArity();
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

                if (_builtinTypes.ContainsKey(s.ToDisplayString()))
                    return _builtinTypes[s.ToDisplayString()];

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