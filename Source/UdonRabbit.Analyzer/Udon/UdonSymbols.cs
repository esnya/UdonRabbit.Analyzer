using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.CodeAnalysis;

using UdonRabbit.Analyzer.Extensions;
using UdonRabbit.Analyzer.Udon.Reflection;

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
            $"{UdonConstants.UdonSharpBehaviour}.__SendCustomEvent__SystemString__SystemVoid",
            $"{UdonConstants.UdonSharpBehaviour}.__SendCustomNetworkEvent__VRCUdonCommonInterfacesNetworkEventTarget_SystemString__SystemVoid",
            $"{UdonConstants.UdonSharpBehaviour}.__SendCustomEventDelayedSeconds__SystemString_SystemSingle_VRCUdonCommonEnumsEventTiming__SystemVoid",
            $"{UdonConstants.UdonSharpBehaviour}.__SendCustomEventDelayedFrames__SystemString_SystemInt32_VRCUdonCommonEnumsEventTiming__SystemVoid",
            $"{UdonConstants.UdonSharpBehaviour}.__VRCInstantiate_UnityEngineGameObject__UnityEngineGameObject",
            $"{UdonConstants.UdonSharpBehaviour}.__RequestSerialization__SystemVoid",

            $"{UdonConstants.UdonSharpBehaviour}.__GetUdonTypeID__SystemInt64",
            $"{UdonConstants.UdonSharpBehaviour}.__GetUdonTypeID__T__SystemInt64",
            $"{UdonConstants.UdonSharpBehaviour}.__GetUdonTypeName__SystemString",
            $"{UdonConstants.UdonSharpBehaviour}.__GetUdonTypeName__T__SystemString",

            // why?
            $"{UdonConstants.UdonCommonInterfacesReceiver}.__GetProgramVariable__SystemString__T",
            $"{UdonConstants.UdonCommonInterfacesReceiver}.__SetProgramVariable__SystemString_T__SystemVoid"
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

        private static readonly HashSet<Type> UdonSyncTypes = new()
        {
            typeof(bool),
            typeof(char),
            typeof(byte),
            typeof(sbyte),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(short),
            typeof(ushort),
            typeof(string)
        };

        private static readonly HashSet<string> UdonSyncTypeFullName = new()
        {
            "UnityEngine.Vector2",
            "UnityEngine.Vector3",
            "UnityEngine.Vector4",
            "UnityEngine.Quaternion",
            "UnityEngine.Color32",
            "UnityEngine.Color",
            "VRC.SDKBase.VRCUrl"
        };

        private readonly Dictionary<string, string> _builtinEvents;
        private readonly Dictionary<Type, Type> _inheritedTypeMap;
        private readonly HashSet<string> _nodeDefinitions;
        private readonly Dictionary<ITypeSymbol, Type> _symbolToTypeMappings;
        private readonly UdonNetworkTypes _udonNetworkTypes;

        public static UdonSymbols Instance { get; private set; }

        private UdonSymbols()
        {
            var manager = new UdonEditorManager(UdonAssemblyLoader.UdonEditorAssembly);
            _builtinEvents = manager.GetBuiltinEvents();
            _inheritedTypeMap = manager.GetVrcInheritedTypeMaps();
            _nodeDefinitions = new HashSet<string>(manager.GetUdonNodeDefinitions());
            _symbolToTypeMappings = new Dictionary<ITypeSymbol, Type>();
            _udonNetworkTypes = new UdonNetworkTypes(UdonAssemblyLoader.UdonAssembly);
        }

        public static void Initialize()
        {
            lock (LockObjForAsmLoad)
            {
                if (Instance != null)
                    return;

                Instance = new UdonSymbols();
            }
        }

        public bool FindUdonMethodName(SemanticModel model, IMethodSymbol symbol)
        {
            var receiver = symbol.ReceiverType;
            if (UdonSharpBehaviourUtility.IsUserDefinedTypes(model, receiver))
                return true;

            var t = RemapVrcBaseTypes(ConvertTypeSymbolToType(receiver));
            var functionNamespace = SanitizeTypeName(t.FullName).Replace(UdonConstants.UdonBehaviour, UdonConstants.UdonCommonInterfacesReceiver);
            var functionName = $"__{symbol.Name.Trim('_').TrimStart('.')}";

            if (functionName == "__VRCInstantiate")
            {
                functionNamespace = "VRCInstantiate";
                functionName = "__Instantiate";
            }

            var paramsSb = new StringBuilder();
            var parameters = symbol.ConstructedFrom.Parameters;

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

            var t = RemapVrcBaseTypes(ConvertTypeSymbolToType(typeSymbol));
            var functionNamespace = SanitizeTypeName(t.FullName).Replace(UdonConstants.UdonBehaviour, UdonConstants.UdonCommonInterfacesReceiver);
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

            var t = RemapVrcBaseTypes(ConvertTypeSymbolToType(typeSymbol));
            var functionNamespace = SanitizeTypeName(t.FullName).Replace(UdonConstants.UdonBehaviour, UdonConstants.UdonCommonInterfacesReceiver);
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
            if (@namespace.CountOf("Array") >= 2)
                @namespace = @namespace.Substring(0, @namespace.IndexOf("Array", StringComparison.InvariantCulture) + "Array".Length); // fix for jagged array

            var signatureForType = $"Type_{@namespace}";
            var signatureForVariable = $"Variable_{@namespace}";
            if (signatureForType == "Type_SystemVoid")
                return true;

            return _nodeDefinitions.Contains(signatureForType) || _nodeDefinitions.Contains(signatureForVariable);
        }

        public bool FindUdonSyncType(ITypeSymbol symbol, string syncMode)
        {
            var t = ConvertTypeSymbolToType(symbol);
            if (t == null)
                return false;

            if (_udonNetworkTypes.IsSupported)
                return syncMode switch
                {
                    "NotSynced" => true,
                    "Linear" => _udonNetworkTypes.CanSync(t) && _udonNetworkTypes.CanSyncLinear(t),
                    "Smooth" => _udonNetworkTypes.CanSync(t) && _udonNetworkTypes.CanSyncSmooth(t),
                    "None" => _udonNetworkTypes.CanSync(t),
                    _ => throw new ArgumentException(nameof(syncMode))
                };

            return UdonSyncTypes.Contains(t) || UdonSyncTypeFullName.Contains(t.FullName);
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
            if (t == null && HasTypeParameter(symbol))
                return ToTypeParameterString(symbol, isSkipBaseTypeRemap);
            if (t == null)
                return string.Empty;

            if (reference == RefKind.None)
                return GetUdonNamedType(t, isSkipBaseTypeRemap);
            return GetUdonNamedType(t.MakeByRefType(), isSkipBaseTypeRemap);
        }

        private string GetUdonNamedType(ITypeSymbol symbol, bool isSkipBaseTypeRemap = false)
        {
            var t = ConvertTypeSymbolToType(symbol);
            if (t == null && HasTypeParameter(symbol))
                return ToTypeParameterString(symbol, isSkipBaseTypeRemap);
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

            var @namespace = e.Namespace;
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

        private static bool HasTypeParameter(ITypeSymbol symbol)
        {
            return symbol switch
            {
                ITypeParameterSymbol => true,
                IArrayTypeSymbol { ElementType: IArrayTypeSymbol } a => HasTypeParameter(a.ElementType),
                IArrayTypeSymbol { ElementType: ITypeParameterSymbol } => true,
                _ => false
            };
        }

        private string ToTypeParameterString(ITypeSymbol symbol, bool isSkipBaseTypeRemap = false)
        {
            return symbol switch
            {
                ITypeParameterSymbol => symbol.Name,
                IArrayTypeSymbol a when a.ElementType is IArrayTypeSymbol || a.ElementType is ITypeParameterSymbol => $"{ToTypeParameterString(a.ElementType, isSkipBaseTypeRemap)}Array",
                _ => GetUdonNamedType(ConvertTypeSymbolToType(symbol), isSkipBaseTypeRemap)
            };
        }

        private ITypeSymbol GetArrayElementTypeSymbol(ITypeSymbol t)
        {
            if (t is not IArrayTypeSymbol a)
                return t;
            return GetArrayElementTypeSymbol(a.ElementType);
        }

        private Type ConvertTypeSymbolToType(ITypeSymbol s)
        {
            var p = GetArrayElementTypeSymbol(s);

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

                if (BuiltinTypes.ContainsKey(p.ToDisplayString()))
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