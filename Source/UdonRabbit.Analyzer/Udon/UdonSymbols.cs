using System;
using System.Collections.Generic;
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

        public static void Initialize()
        {
            lock (LockObjForAsmLoad)
            {
                if (Instance != null)
                    return;

                var manager = new UdonEditorManager(UdonAssemblyLoader.UdonAssembly);
                Instance = new UdonSymbols(manager);
            }
        }

        public bool FindUdonMethodName(SemanticModel model, IMethodSymbol symbol)
        {
            var receiver = symbol.ReceiverType;
            if (UdonSharpBehaviourUtility.IsUserDefinedTypes(model, receiver))
                return true;

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
                returnsSb.Append($"__{GetUdonNamedType(symbol.ConstructedFrom.ReturnType, true)}");

            var signature = $"{functionNamespace}.{functionName}{paramsSb}{returnsSb}";

            return AllowMethodNameList.Contains(signature) || _nodeDefinitions.Contains(signature);
        }

        public bool FindUdonVariableName(SemanticModel model, ITypeSymbol typeSymbol, IFieldSymbol fieldSymbol, bool isSetter)
        {
            if (UdonSharpBehaviourUtility.IsUserDefinedTypes(model, typeSymbol))
                return true;

            var functionNamespace = SanitizeTypeName(typeSymbol.ToDisplayString()).Replace(UdonConstants.UdonBehaviour, UdonConstants.UdonCommonInterfacesReceiver);
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
            if (UdonSharpBehaviourUtility.IsUserDefinedTypes(model, typeSymbol))
                return true;

            var functionNamespace = SanitizeTypeName(typeSymbol.ToDisplayString()).Replace(UdonConstants.UdonBehaviour, UdonConstants.UdonCommonInterfacesReceiver);
            if (AllowClassNameList.Contains(functionNamespace))
                return true;

            var functionName = $"__{(isSetter ? "set" : "get")}_{symbol.Name.Trim('_')}";
            var param = $"__{GetUdonNamedType(symbol.Type)}";
            if (isSetter)
                param += "__SystemVoid";

            var signature = $"{functionNamespace}.{functionName}{param}";
            return _nodeDefinitions.Contains(signature);
        }

        public bool FindUdonTypeName(SemanticModel model, ITypeSymbol typeSymbol)
        {
            if (UdonSharpBehaviourUtility.IsUserDefinedTypes(model, typeSymbol))
                return true;
            if (typeSymbol.TypeKind == TypeKind.Array && UdonSharpBehaviourUtility.IsUserDefinedTypes(model, typeSymbol, TypeKind.Array))
                return true;

            var @namespace = GetUdonNamedType(typeSymbol);
            var signatureForType = $"Type_{@namespace}";
            var signatureForVariable = $"Variable_{@namespace}";
            if (signatureForType == "Type_SystemVoid")
                return true;

            return _nodeDefinitions.Contains(signatureForType) || _nodeDefinitions.Contains(signatureForVariable);
        }

        private Type RemapVrcBaseTypes(Type t)
        {
            var depth = 0;
            var current = t;

            while (current!.IsArray)
            {
                current = current.GetElementType();
                depth++;
            }

            if (!_inheritedTypeMap.ContainsKey(current))
                return t;

            t = _inheritedTypeMap[current];
            while (depth-- > 0)
                t = t.MakeArrayType();

            return t;
        }

        private string GetUdonNamedType(ITypeSymbol symbol, RefKind reference, bool isSkipBaseTypeRemap = false)
        {
            var t = ConvertTypeSymbolToType(symbol);
            if (t == null && symbol is ITypeParameterSymbol s)
                return s.Name;
            if (t == null)
                return string.Empty;

            if (reference == RefKind.None)
                return GetUdonNamedType(t, isSkipBaseTypeRemap);
            return GetUdonNamedType(t.MakeByRefType(), isSkipBaseTypeRemap);
        }

        private string GetUdonNamedType(ITypeSymbol symbol, bool isSkipBaseTypeRemap = false)
        {
            var t = ConvertTypeSymbolToType(symbol);
            if (t == null && symbol is ITypeParameterSymbol s)
                return s.Name;
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
            var p = s switch
            {
                IArrayTypeSymbol a when s.TypeKind == TypeKind.Array => a.ElementType,
                _ => s
            };

            Type ConvertToTypeInternal(Type t)
            {
                static Type ConvertArrayType(IArrayTypeSymbol a1, Type t1)
                {
                    var current = (ITypeSymbol) a1;
                    while (current.TypeKind == TypeKind.Array)
                    {
                        t1 = t1.MakeArrayType();
                        current = ((IArrayTypeSymbol) current).ElementType;
                    }

                    return t1;
                }

                return s switch
                {
                    IArrayTypeSymbol a when s.TypeKind == TypeKind.Array => ConvertArrayType(a, t),
                    _ => t
                };
            }

            lock (LockObjForTypeMap)
            {
                if (_symbolToTypeMappings.ContainsKey(p))
                    return ConvertToTypeInternal(_symbolToTypeMappings[p]);

                if (BuiltinTypes.ContainsKey(s.ToDisplayString()))
                    return ConvertToTypeInternal(BuiltinTypes[p.ToDisplayString()]);

                static IEnumerable<Type> LoadExportedTypes(Assembly asm)
                {
                    try
                    {
                        var types = new List<Type>();
                        types.AddRange(asm.GetTypes());

                        foreach (var type in asm.GetTypes())
                            types.AddRange(type.GetNestedTypes());

                        return types;
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        return e.Types;
                    }
                }

                if (_symbolToTypeMappings.ContainsKey(p))
                    return _symbolToTypeMappings[p];

                var t = AppDomain.CurrentDomain.GetAssemblies()
                                 .SelectMany(LoadExportedTypes)
                                 .Where(w => !string.IsNullOrWhiteSpace(w?.FullName))
                                 .FirstOrDefault(w => w.FullName.Replace("+", ".") == p.ToDisplayString());
                if (t == null)
                    return null;

                _symbolToTypeMappings.Add(p, t);
                return ConvertToTypeInternal(t);
            }
        }
    }
}