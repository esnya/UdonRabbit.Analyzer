using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UdonRabbit.Analyzer.Udon
{
    public class UdonAssemblyResolver
    {
        private readonly HashSet<string> _assemblies;
        private readonly Dictionary<string, bool> _paths;

        public UdonAssemblyResolver(IEnumerable<string> paths)
        {
            _assemblies = new HashSet<string>();
            _paths = paths.ToDictionary(w => w, _ => false);
        }

        public string Resolve(string name)
        {
            if (_assemblies.Any(w => w.EndsWith(name)))
                return _assemblies.First(w => w.EndsWith(name));

            var dict = new Dictionary<string, bool>(_paths);

            foreach (var path in dict.Where(w => !w.Value))
            {
                var assemblies = Directory.GetFiles(path.Key, "*.dll", SearchOption.AllDirectories);
                foreach (var assembly in assemblies)
                    _assemblies.Add(assembly);

                _paths[path.Key] = true;

                if (assemblies.Any(w => w.EndsWith(name)))
                    return assemblies.First(w => w.EndsWith(name));
            }

            return null;
        }
    }
}