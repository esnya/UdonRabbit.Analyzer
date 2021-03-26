using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UdonRabbit.Analyzer.Udon
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
            var dict = new Dictionary<Type, Type>();
            var implAssembly = AppDomain.CurrentDomain.GetAssemblies().First(w => w.GetName().Name == "VRCSDK3");
            var baseAssembly = AppDomain.CurrentDomain.GetAssemblies().First(w => w.GetName().Name == "VRCSDKBase");

            // Why manually added? -> because throw exceptions on GetTypes(), GetExportedTypes() and get_Namespace().

            dict.Add(implAssembly.GetType("VRC.SDK3.Components.AbstractUdonBehaviour"), baseAssembly.GetType("VRC.SDKBase.INetworkID"));
            dict.Add(implAssembly.GetType("VRC.SDK3.Components.IVRCDestructible"), baseAssembly.GetType("VRC.SDKBase.IVRC_Destructible"));
            dict.Add(implAssembly.GetType("VRC.SDK3.Components.VRCAvatarPedestal"), baseAssembly.GetType("VRC.SDKBase.VRC_AvatarPedestal"));
            dict.Add(implAssembly.GetType("VRC.SDK3.Components.VRCInteractable"), baseAssembly.GetType("VRC.SDKBase.VRC_Interactable"));
            dict.Add(implAssembly.GetType("VRC.SDK3.Components.VRCMirrorReflection"), baseAssembly.GetType("VRC.SDKBase.VRC_MirrorReflection"));
            dict.Add(implAssembly.GetType("VRC.SDK3.Components.VRCObjectPool"), baseAssembly.GetType("VRC.SDKBase.INetworkID"));
            dict.Add(implAssembly.GetType("VRC.SDK3.Components.VRCObjectSync"), baseAssembly.GetType("VRC.SDKBase.INetworkID"));
            dict.Add(implAssembly.GetType("VRC.SDK3.Components.VRCPickup"), baseAssembly.GetType("VRC.SDKBase.VRC_Pickup"));
            dict.Add(implAssembly.GetType("VRC.SDK3.Components.VRCPortalMarker"), baseAssembly.GetType("VRC.SDKBase.VRC_PortalMarker"));
            dict.Add(implAssembly.GetType("VRC.SDK3.Components.VRCSceneDescriptor"), baseAssembly.GetType("VRC.SDKBase.VRC_SceneDescriptor"));
            dict.Add(implAssembly.GetType("VRC.SDK3.Components.VRCSpatialAudioSource"), baseAssembly.GetType("VRC.SDKBase.VRC_SpatialAudioSource"));
            dict.Add(implAssembly.GetType("VRC.SDK3.Components.VRCStation"), baseAssembly.GetType("VRC.SDKBase.VRCStation"));
            dict.Add(implAssembly.GetType("VRC.SDK3.Components.VRCUiShape"), baseAssembly.GetType("VRC.SDKBase.VRC_UiShape"));
            dict.Add(implAssembly.GetType("VRC.SDK3.Components.VRCVisualDamage"), baseAssembly.GetType("VRC.SDKBase.VRC_VisualDamage"));
            dict.Add(implAssembly.GetType("VRC.SDK3.Video.Components.VRCUnityVideoPlayer"), implAssembly.GetType("VRC.SDK3.Video.Components.Base.BaseVRCVideoPlayer"));
            dict.Add(implAssembly.GetType("VRC.SDK3.Video.Components.AVPro.VRCAVProVideoPlayer"), implAssembly.GetType("VRC.SDK3.Video.Components.Base.BaseVRCVideoPlayer"));

            return dict;
        }
    }
}