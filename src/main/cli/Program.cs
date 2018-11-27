using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;

namespace Azurlane
{
    internal enum Tasks
    {
        Encrypt,
        EncryptDevelopment,
        Decrypt,
        DecryptDevelopment,
        Decompile,
        DecompileDevelopment,
        Recompile,
        RecompileDevelopment,
        Unpack,
        UnpackDevelopment,
        Repack,
        RepackDevelopment
    }

    public class Program
    {
        internal static bool Ok = false;

        private static readonly List<string> ListOfAssetBundle = new List<string>(), ListOfLua = new List<string>();
        private static readonly Dictionary<string, List<string>> Parameters = new Dictionary<string, List<string>>();
        private static string _currentOption;

        internal static void Main(string[] args)
        {
            // Check whether arguments are valid by counting the length, if < 2 = show help message
            var showHelp = args.Length < 2;

            // Initialize options
            var options = new OptionSet()
            {
                {"u|unlock", "Decrypt Lua", v => _currentOption = "lua.unlock"},
                {"l|lock", "Encrypt Lua", v => _currentOption = "lua.lock"},
                {"d|decompile", "Decompile Lua", v => _currentOption = "lua.decompile"},
                {"r|recompile", "Recompile Lua", v => _currentOption = "lua.recompile"},
                {"decrypt", "Decrypt AssetBundle",  v => _currentOption = "assetbundle.decrypt"},
                {"encrypt", "Encrypt AssetBundle", v => _currentOption = "assetbundle.encrypt"},
                {"unpack", "Unpack all lua from AssetBundle", v => _currentOption = "assetbundle.unpack"},
                {"repack", "Repack all lua from AssetBundle", v => _currentOption = "assetbundle.repack"},
                {"u2|unlock2", "Decrypt Lua (Development)", v => _currentOption = "lua.dev.unlock"},
                {"l2|lock2", "Encrypt Lua (Development)", v => _currentOption = "lua.dev.lock"},
                {"d2|decompile2", "Decompile Lua (Development)", v => _currentOption = "lua.dev.decompile"},
                {"r2|recompile2", "Recompile Lua (Development)", v => _currentOption = "lua.dev.recompile"},
                {"decrypt2", "Decrypt AssetBundle (Development)",  v => _currentOption = "assetbundle.dev.decrypt"},
                {"encrypt2", "Encrypt AssetBundle (Development)", v => _currentOption = "assetbundle.dev.encrypt"},
                {"unpack2", "Unpack all lua from AssetBundle (Development)", v => _currentOption = "assetbundle.dev.unpack"},
                {"repack2", "Repack all lua from AssetBundle (Development)", v => _currentOption = "assetbundle.dev.repack"},
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

            // If showHelp return true, then print message and abort
            if (showHelp)
            {
                Help(options); // print message
                return; // abort
            }

            try
            {
                options.Parse(args); // Trying to parse options
            }
            catch (OptionException e)
            {
                // Catch unexpected exception
                Utils.LogException("Exception detected during parsing options", e);
            }

            // Iterate through parameter in parameters (dictionary)
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

            // Begin to initialize ConfigMgr
            ConfigMgr.Initialize();

            if (OpContains("lua."))
            {
                foreach (var lua in ListOfLua)
                    LuaMgr.Initialize(lua, OpContains("lua.dev.", "unlock") ? Tasks.DecryptDevelopment : OpContains("lua.", "unlock") ? Tasks.Decrypt : OpContains("lua.dev.", "lock") ? Tasks.EncryptDevelopment : OpContains("lua.", "lock") ? Tasks.Encrypt : OpContains("lua.dev.", "unpack") ? Tasks.UnpackDevelopment : OpContains("lua.", "unpack") ? Tasks.Unpack : OpContains("lua.dev", "repack") ? Tasks.RepackDevelopment : Tasks.Repack);
            }
            else if (OpContains("assetbundle."))
            {
                foreach (var assetbundle in ListOfAssetBundle)
                    AssetBundleMgr.Initialize(assetbundle, OpContains("assetbundle.dev.", "decrypt") ? Tasks.DecryptDevelopment : OpContains("assetbundle.", "decrypt") ? Tasks.Decrypt : OpContains("assetbundle.dev.", "encrypt") ? Tasks.EncryptDevelopment : OpContains("assetbundle.", "encrypt") ? Tasks.Encrypt : OpContains("assetbundle.dev.", "unpack") ? Tasks.UnpackDevelopment : OpContains("assetbundle.", "unpack") ? Tasks.Unpack : OpContains("assetbundle.dev.", "repack") ? Tasks.RepackDevelopment : Tasks.Repack);
            }

            if (Ok && !OpContains(".repack") && !OpContains(".decrypt") && !OpContains(".encrypt"))
            {
                Console.WriteLine();
                Utils.WriteLine($"{(OpContains(".unlock") || OpContains(".decrypt") ? "Decrypt" : OpContains(".lock") || OpContains(".encrypt") ? "Encrypt" : OpContains(".decompile") ? "Decompile" : OpContains(".recompile") ? "Recompile" : OpContains(".unpack") ? "Unpacking" : "Repacking")} {(OpContains("lua.dev.") ? "" : "assetbundle ")}is done");

                if (!OpContains("dev.") && !OpContains(".unpack"))
                {
                    Utils.WriteLine("Success: {0} - Failed: {1}", LuaMgr.SuccessCount, LuaMgr.FailedCount);
                }
            }
        }

        /// <summary>
        /// Print help message to the terminal
        /// </summary>
        /// <param name="options"></param>
        private static void Help(OptionSet options)
        {
            Utils.WriteLine("[Info]> Usage: Azurlane.exe <option> <path-to-file(s) or path-to-directory(s)>");
            Console.WriteLine();
            Utils.WriteLine("Options:");
            options.WriteOptionDescriptions(Console.Out);
        }

        private static bool OpContains(string key) => OpContains(key);

        private static bool OpContains(string key1, string key2) => OpContains(key1) && OpContains(key2);
    }
}