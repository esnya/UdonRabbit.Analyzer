using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace UdonRabbit.Analyzer.Udon
{
    public class UdonAssemblyResolver
    {
        private readonly HashSet<string> _assemblies;
        private readonly Dictionary<string, bool> _paths;
        private readonly string _session;
        private readonly string _unique;
        private string _sessionDir;

        public UdonAssemblyResolver(IEnumerable<string> paths)
        {
            _assemblies = new HashSet<string>();
            _paths = paths.ToDictionary(w => w, _ => false);

            _session = $"SDK@{UdonAssemblyVersion.SDKVersion}-Udon@{UdonAssemblyVersion.UdonVersion}-UdonSharp@{UdonAssemblyVersion.UdonSharpVersion}";

            var md5 = new MD5CryptoServiceProvider();
            var sb = new StringBuilder();
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(_paths.First().Key));
            foreach (var b in bytes)
                sb.Append(b.ToString("X2"));

            _unique = sb.ToString();
        }

        public string Resolve(string name)
        {
            if (_assemblies.Any(w => w.EndsWith(name)))
                return _assemblies.First(w => w.EndsWith(name));

            var dict = new Dictionary<string, bool>(_paths);

            foreach (var path in dict.Where(w => !w.Value))
            {
                var baseDir = string.IsNullOrEmpty(_sessionDir) ? Path.GetFullPath(Path.Combine(Path.GetTempPath(), "UdonRabbit.Analyzer", _unique, _session)) : _sessionDir;
                if (!Directory.Exists(baseDir))
                {
                    Directory.CreateDirectory(baseDir);
                    _sessionDir = baseDir;
                }

                var assemblies = Directory.GetFiles(path.Key, "*.dll", SearchOption.AllDirectories);
                foreach (var assembly in assemblies)
                    if (assembly.Contains("ScriptAssemblies"))
                    {
                        var dest = Path.Combine(baseDir, Path.GetFileName(assembly));
                        try
                        {
                            File.Copy(assembly, dest, true);
                        }
                        catch
                        {
                            // ignored
                        }

                        _assemblies.Add(dest);
                    }
                    else
                    {
                        _assemblies.Add(assembly);
                    }

                _paths[path.Key] = true;

                if (assemblies.Any(w => w.EndsWith(name)))
                    return assemblies.First(w => w.EndsWith(name));
            }

            return null;
        }

        public void Cleanup()
        {
            // Nothing To Do
        }
    }
}