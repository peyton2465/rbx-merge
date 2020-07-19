using RobloxFiles;
using System;
using System.IO;
using System.CommandLine;
using System.Threading.Tasks;
using System.CommandLine.Invocation;

namespace rbx_merge
{
    public class Program
    { 
        public static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("Merges required Roblox modules into a single script.") {
                new Option<string>(new string[] { "--input", "-i" }) {
                    Description = "Input file (.rbxm, .rbxl, .rbxlx)",
                    Argument = new Argument<string>(),
                    Required = true
                },
                new Option(new string[] { "--path", "-p" }) {
                    Description = "Path to main script (ex: game.StarterGui.LocalScript)",
                    Argument = new Argument<string>(),
                    Required = true
                },
                new Option(new string[] { "--minify", "-m" }) {
                    Description = "Minify the final script (default: true) (trust me, you want this)",
                    Argument = new Argument<string>(),
                    Required = false
                },
                new Option(new string[] { "--output", "-o" }) {
                    Description = "Output file (default: merged.lua)",
                    Argument = new Argument<string>(),
                    Required = false
                }
            };

            rootCommand.Handler = CommandHandler.Create<string, string, bool, string>(Merge);
            return await rootCommand.InvokeAsync(args);
        }

        static public void Merge(string input, string path, bool minify = true, string output = "merged.lua")
        {
#if RELEASE
            try
            {
#endif
                if (!File.Exists(input)) throw new Exception($"Unable to locate file '{input}'");

                Logs.Output("Reading file '{0}'", input);
                var root = RobloxFile.Open(File.OpenRead(input));
                var parser = new Parser(root);

                Logs.Output("Parsing path '{0}'", path);
                var selection = parser.ParsePath(path);
                if (!selection.IsA<BaseScript>()) throw new Exception($"Selected instance '{path}' is not a BaseScript");

                var merged = Merger.Merge(parser, (BaseScript)selection, minify);

                var outputFile = File.CreateText(output);
                outputFile.Write(merged);
                outputFile.Close();

                Logs.Output("Merge complete! Output located at '{0}'", output);
#if RELEASE
            }
            catch (Exception e)
            {
                Logs.Info(e.Message);
            }
#endif
        }
    }
}
