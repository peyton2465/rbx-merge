using RobloxFiles;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace rbx_merge
{
    public class Parser
    {
        private const string _pathPattern = @"(.*?)(\.|\[(\""|\')|(\""|\')\]|\Z)";
        private const string _importsPattern = @"require[\s(]*([\w\.""'\[\] ]+)(?=\srequire)?";

        private Instance _root;
        public Parser(RobloxFile root)
        {
            _root = root;
        }

        private Instance ParseIndex(Instance instance, string index)
        {
            switch (index)
            {
                case "game":
                    return _root;
                case "script":
                    return instance;
                case "Parent":
                    return instance.Parent;
                default:
                    return instance.FindFirstChild(index);
            }
        }

        public Dictionary<Instance, Match> ParseImports(Instance script)
        {
            var imports = new Dictionary<Instance, Match>();
            foreach (Match match in Regex.Matches(script.GetProperty("Source").Value.ToString(), _importsPattern))
            {
                var path = match.Groups[1].Value;
                if (path.Length > 0)
                {
                    var import = ParsePath(path, script);
                    if (import == null) throw new Exception($"Unable to locate import '{path}' in script '{script.GetFullName()}'");
                    imports.Add(import, match);
                }
            }
            return imports;
        }

        public Instance ParsePath(string input, Instance startingInstance = null)
        {
            Instance currentInstance = startingInstance == null ? _root : startingInstance;
            foreach(Match match in Regex.Matches(input, _pathPattern))
            {
                var index = match.Groups[1].Value;
                if(index.Length > 0)
                    currentInstance = ParseIndex(currentInstance, index);
            }
            return currentInstance;
        }
    }
}
