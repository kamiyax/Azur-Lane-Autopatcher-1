using System;
using System.Collections.Generic;
using System.IO;
using NDesk.Options;

namespace Azurlane
{
    internal enum Tasks
    {
        Encrypt,
        Decrypt,
        Decompile,
        Recompile,
        Unpack,
        Repack
    }

    public class Program
    {
        private static readonly List<string> ListOfAssetBundle = new List<string>(), ListOfLua = new List<string>();
        private static readonly Dictionary<string, List<string>> Parameters = new Dictionary<string, List<string>>();
        private static string _currentOption;

        private static void HelpMessage(OptionSet options)
        {
            Utils.WriteLine("Usage: Azurlane.exe <option> <path-to-file(s) or path-to-directory(s)>");
            Console.WriteLine();
            Utils.WriteLine("Options:");
            options.WriteOptionDescriptions(Console.Out);
        }

        internal static void Main(string[] args)
        {
            var showHelp = args.Length < 2;
            var options = new OptionSet()
            {
                {"u|unlock", "Decrypt Lua", v => _currentOption = "lua.unlock"},
                {"l|lock", "Encrypt Lua", v => _currentOption = "lua.lock"},
                {"d|decompile", "Decompile Lua (will automatically decrypt if Lua is encrypted)", v => _currentOption = "lua.decompile"},
                {"r|recompile", "Recompile Lua", v => _currentOption = "lua.recompile"},
                {"decrypt", "Decrypt AssetBundle",  v => _currentOption = "assetbundle.decrypt"},
                {"encrypt", "Encrypt AssetBundle", v => _currentOption = "assetbundle.encrypt"},
                {"unpack", "Unpack all lua from AssetBundle (will automatically decrypt if AssetBundle is encrypted)", v => _currentOption = "assetbundle.unpack"},
                {"repack", "Repack all lua from AssetBundle", v => _currentOption = "assetbundle.repack"},
                {"<>", v => {
                    if (_currentOption == null) {
                        showHelp = true;
                        return;
                    }

                    if (Parameters.TryGetValue(_currentOption, out var values))
                    {
                        values.Add(v);
                    }
                    else
                    {
                        values = new List<string> { v };
                        Parameters.Add(_currentOption, values);
                    }
                }}
            };

            if (showHelp)
            {
                HelpMessage(options);
                return;
            }

            try
            {
                options.Parse(args);
            }
            catch (OptionException e)
            {
                Utils.LogException("Exception detected during parsing options", e);
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