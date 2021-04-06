using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UdonRabbit.Analyzer.Extensions;

namespace UdonRabbit.Analyzer.Udon.Reflection
{
    public class UdonEditorManager
    {
        private readonly object _instance;

        public bool HasInstance => _instance != null;

        public UdonEditorManager(Assembly asm)
        {
            var t = asm.GetType("VRC.Udon.Editor.UdonEditorManager");
            var p = t.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            if (p == null)
                return;

            _instance = p.GetValue(null);
        }

        public IEnumerable<string> GetUdonNodeDefinitions()
        {
            var m = _instance.GetType().GetMethod("GetNodeDefinitions", Type.EmptyTypes);
            if (m == null)
                return new List<string>();

            var definitions = m.Invoke(_instance, new object[] { }) as IEnumerable<object>;
            return (definitions ?? Array.Empty<object>()).Select(w =>
            {
                var p = w.GetType().GetField("fullName", BindingFlags.Public | BindingFlags.Instance);
                return p?.GetValue(w) as string;
            }).Where(w => !string.IsNullOrWhiteSpace(w)).Distinct();
        }

        public Dictionary<string, string> GetBuiltinEvents()
        {
            var m = _instance.GetType().GetMethod("GetNodeDefinitions", new[] { typeof(string) });
            if (m == null)
                return new Dictionary<string, string>();

            var definitions = m.Invoke(_instance, new object[] { "Event_" }) as IEnumerable<object>;
            return (definitions ?? Array.Empty<object>()).Select(w =>
            {
                var p = w.GetType().GetField("fullName", BindingFlags.Public | BindingFlags.Instance);
                var fullName = p?.GetValue(w) as string;

                return fullName == "Event_Custom" ? null : fullName?.Substring(6);
            }).Where(w => !string.IsNullOrWhiteSpace(w)).Distinct().ToDictionary(w => w, w => $"_{char.ToLowerInvariant(w[0])}{w.Substring(1)}");
        }

        public Dictionary<Type, Type> GetVrcInheritedTypeMaps()
        {
            var implAssembly = AppDomain.CurrentDomain.GetAssemblies().First(w => w.GetName().Name == "VRCSDK3");
            var mappingTypes = implAssembly.GetTypes().Where(w => w.Namespace != null && w.Namespace.StartsWith("VRC.SDK3.Components"));
            var dict = mappingTypes.Where(t => t.BaseType != null && t.BaseType.Namespace != null && t.BaseType.Namespace.StartsWith("VRC.SDKBase"))
                                   .ToDictionary(t => t.BaseType);

            dict.AddIfValid(implAssembly.GetType("VRC.SDK3.Video.Components.VRCUnityVideoPlayer"), implAssembly.GetType("VRC.SDK3.Video.Components.Base.BaseVRCVideoPlayer"));
            dict.AddIfValid(implAssembly.GetType("VRC.SDK3.Video.Components.AVPro.VRCAVProVideoPlayer"), implAssembly.GetType("VRC.SDK3.Video.Components.Base.BaseVRCVideoPlayer"));

            return dict;
        }
    }
}