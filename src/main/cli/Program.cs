using System;
using System.Collections.Generic;
using System.IO;
using NDesk.Options;

namespace Azurlane
{
    /// <summary>
    /// Tasks
    /// </summary>
    internal enum Tasks
    {
        Encrypt,
        Decrypt,
        Decompile,
        Recompile,
        Unpack,
        Repack
    }

    /// <summary>
    /// Program
    /// </summary>
    public class Program
    {
        private static readonly List<string> ListOfAssetBundle = new List<string>(), ListOfLua = new List<string>();
        private static readonly Dictionary<string, List<string>> Parameters = new Dictionary<string, List<string>>();
        private static string CurrentOption;

        private static void HelpMessage(OptionSet options)
        {
            Console.WriteLine("Usage: Azurlane.exe <option> <path-to-file(s) or path-to-directory(s)>");
            Console.WriteLine(">!You can input multiple files or directory, lua & assetbundle are the only acceptable file!<");
            Console.WriteLine();
            Console.WriteLine("Options:");
            options.WriteOptionDescriptions(Console.Out);
        }

        internal static void Main(string[] args)
        {
            var showHelp = args.Length < 2;
            var options = new OptionSet()
            {
                {"u|unlock", "Decrypt Lua", v => CurrentOption = "lua.unlock"},
                {"l|lock", "Encrypt Lua", v => CurrentOption = "lua.lock"},
                {"d|decompile", "Decompile Lua (will automatically decrypt if Lua is encrypted)", v => CurrentOption = "lua.decompile"},
                {"r|recompile", "Recompile Lua", v => CurrentOption = "lua.recompile"},
                {"decrypt", "Decrypt AssetBundle",  v => CurrentOption = "assetbundle.decrypt"},
                {"encrypt", "Encrypt AssetBundle", v => CurrentOption = "assetbundle.encrypt"},
                {"unpack", "Unpack all lua from AssetBundle (will automatically decrypt if AssetBundle is encrypted)", v => CurrentOption = "assetbundle.unpack"},
                {"repack", "Repack all lua from AssetBundle", v => CurrentOption = "assetbundle.repack"},
                {"<>", v => {
                    if (CurrentOption == null) {
                        showHelp = true;
                        return;
                    }

                    if (Parameters.TryGetValue(CurrentOption, out var values))
                    {
                        values.Add(v);
                    }
                    else
                    {
                        values = new List<string> { v };
                        Parameters.Add(CurrentOption, values);
                    }
                }}
            };

            if (showHelp)
            {
                HelpMessage(options);
                return;
            }
            else
            {
                try
                {
                    options.Parse(args);
                }
                catch (OptionException e)
                {
                    Utils.LogException("Exception detected during parsing options", e);
                }
            }

            foreach (var parameter in Parameters)
            {
                foreach (var value in parameter.Value)
                {
                    if (!File.Exists(value) && !Directory.Exists(value))
                    {
                        Utils.WriteLine($@"A file or directory named {value} does not exists.");
                    }
                    if (File.Exists(value))
                    {
                        if (parameter.Key.Contains("lua.")) ListOfLua.Add(Path.GetFullPath(value));
                        else ListOfAssetBundle.Add(Path.GetFullPath(value));
                    }
                    else if (Directory.Exists(value))
                    {
                        if (parameter.Key.Contains("lua."))
                        {
                            foreach (var file in Directory.GetFiles(Path.GetFullPath(value), "*.lua*", SearchOption.AllDirectories))
                                ListOfLua.Add(file);
                        }
                        else
                        {
                            foreach (var file in Directory.GetFiles(Path.GetFullPath(value), "*", SearchOption.AllDirectories))
                                ListOfAssetBundle.Add(file);
                        }
                    }
                }
            }

            ConfigMgr.Initialize();
        }
    }
}