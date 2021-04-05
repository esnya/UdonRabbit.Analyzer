using System;
using System.Reflection;

namespace UdonRabbit.Analyzer.Udon.Reflection
{
    public class UdonNetworkTypes
    {
        private readonly Type _instance;
        private MethodInfo _delegateOfCanSync;
        private MethodInfo _delegateOfCanSyncLinear;
        private MethodInfo _delegateOfCanSyncSmooth;

        public bool IsSupported => _instance != null;

        public UdonNetworkTypes(Assembly asm)
        {
            var t = asm.GetType("VRC.Udon.UdonNetworkTypes");
            var m = t?.GetMethod("CanSync", BindingFlags.Public | BindingFlags.Static);
            if (m == null)
                return;

            _instance = t;
        }

        public bool CanSync(Type t)
        {
            _delegateOfCanSync ??= _instance.GetMethod("CanSync", BindingFlags.Public | BindingFlags.Static);
            if (_delegateOfCanSync == null)
                return false;

            return (bool) _delegateOfCanSync.Invoke(null, new object[] { t });
        }

        public bool CanSyncLinear(Type t)
        {
            _delegateOfCanSyncLinear ??= _instance.GetMethod("CanSyncLinear", BindingFlags.Public | BindingFlags.Static);
            if (_delegateOfCanSyncLinear == null)
                return false;

            return (bool) _delegateOfCanSyncLinear.Invoke(null, new object[] { t });
        }

        public bool CanSyncSmooth(Type t)
        {
            _delegateOfCanSyncSmooth ??= _instance.GetMethod("CanSyncSmooth", BindingFlags.Public | BindingFlags.Static);
            if (_delegateOfCanSyncSmooth == null)
                return false;

            return (bool) _delegateOfCanSyncSmooth.Invoke(null, new object[] { t });
        }
    }
}