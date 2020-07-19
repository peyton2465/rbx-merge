using RobloxFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace rbx_merge
{
    public class Merger
    {
        private static Parser _parser;
        private static Dictionary<string, string> _globalImports;

        private static void LoadImports(Instance script)
        {
            var imports = _parser.ParseImports(script);
            foreach (var import in imports)
                LoadImports(import.Key);

            var guid = script.GetProperty("ScriptGuid").Value.ToString();
            if (_globalImports.ContainsKey(guid)) return;

            var newsrc = script.GetProperty("Source").Value.ToString();

            foreach (var import in imports.Reverse())
            {
                var importGuid = import.Key.GetProperty("ScriptGuid").Value.ToString();
                var path = import.Value.Groups[1];
                
                newsrc = newsrc.Substring(0, path.Index) + $"\"{importGuid}\"" + newsrc.Substring(path.Index + path.Length);
            }

            _globalImports.Add(guid, newsrc);
        }
        private static string BuildImportsTable(IEnumerable<KeyValuePair<string, string>> imports)
        {
            var table = "{";
            foreach (var import in imports)
            {
                table += $"\n\t[\"{import.Key}\"] = function(){import.Value} end,";
            }
            return table + "\n}";
        }

        public static string Merge(Parser parser, Instance script, bool minify)
        {
            _parser = parser;
            _globalImports = new Dictionary<string, string>();

            var scriptGuid = script.GetProperty("ScriptGuid").Value.ToString();

            LoadImports(script);
            Logs.Output("Loaded {0} imports {1}", _globalImports.Count - 1, _globalImports.Count == 0 ? ".. uh oh.." : "successfully!");

            Logs.Output("Merging..");
            var importsTable = BuildImportsTable(_globalImports.Where(x => x.Key != scriptGuid));

            var assembly = Assembly.GetExecutingAssembly();
            var resource = assembly.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith("MergeTemplate.lua"));
            if (resource == null || resource.Equals(default)) throw new Exception("Unable to locate merge template");

            using (var stream = assembly.GetManifestResourceStream(resource))
            using (var reader = new StreamReader(stream)) 
            {
                var output = string.Format(reader.ReadToEnd(), importsTable, _globalImports[scriptGuid]);
                if (minify)
                {
                    Logs.Output("Minifying..");
                    output = Minifier.Minify(output);
                }
                return output;
            }
        }
    }
}
