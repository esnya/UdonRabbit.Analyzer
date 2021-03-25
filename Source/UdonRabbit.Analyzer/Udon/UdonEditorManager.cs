using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UdonRabbit.Analyzer.Udon
{
    internal class UdonEditorManager
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
                return p.GetValue(w) as string;
            });
        }
    }
}