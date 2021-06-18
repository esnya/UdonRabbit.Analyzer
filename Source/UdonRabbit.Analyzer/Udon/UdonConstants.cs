using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UdonRabbit.Analyzer.Udon
{
    public static class UdonConstants
    {
        public const string UdonBehaviour = "VRCUdonUdonBehaviour";
        public const string UdonCommonInterfacesReceiver = "VRCUdonCommonInterfacesIUdonEventReceiver";
        public const string UdonSharpBehaviour = "UdonSharpUdonSharpBehaviour";
        public const string UdonSharpBehaviourFullName = "UdonSharp.UdonSharpBehaviour";
        public const string UdonSharpSyncModeFullName = "UdonSharp.UdonSyncMode";
        public const string UdonSharpBehaviourSyncModeFullName = "UdonSharp.BehaviourSyncMode";

        public const string UdonCategory = "Udon";
        public const string UdonSharpCategory = "UdonSharp";
        public const string CompilerCategory = "Compiler";

        public static ReadOnlyCollection<(string, int)> UdonCustomMethodInvokers => new List<(string, int)>
        {
            ("SendCustomEvent", 0),
            ("SendCustomNetworkEvent", 1),
            ("SendCustomEventDelayedSeconds", 0),
            ("SendCustomEventDelayedFrames", 0)
        }.AsReadOnly();

        public static ReadOnlyCollection<(string, int)> UdonCustomNetworkMethodInvokers => new List<(string, int)>
        {
            ("SendCustomNetworkEvent", 1)
        }.AsReadOnly();
    }
}