using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Azurlane
{
    internal enum Tasks
    {
        Encrypt,
        Decrypt,
        Decompile,
        Recompile,
        Unpack,
        Repack,
    }

    public class Program
    {
        internal static bool DevelopmentMode;
        internal static bool Ok = false;
        private static bool _abort;

        private static readonly List<string> ListOfAssetBundle = new List<string>(), ListOfLua = new List<string>();

        //private static readonly Dictionary<string, List<string>> Parameters = new Dictionary<string, List<string>>();
        private static readonly Dictionary<Options, List<string>> Parameters = new Dictionary<Options, List<string>>();

        //private static string _currentOption;
        private static Options _currentOption = Options.None;

        private enum Options
        {
            None,
            LuaUnlock,
            LuaLock,
            LuaDecompile,
            LuaRecompile,
            AssetBundleDecrypt,
            AssetBundleEncrypt,
            AssetBundleUnpack,
            AssetBundleRepack,
        }

        private static void Initialize()
        {
            ConfigMgr.Initialize();

            var missingCount = 0;
            double pythonVersion;

            // Checking python version
            using (var process = new Process())
            {
                process.StartInfo.FileName = "python";
                process.StartInfo.Arguments = "--version";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;

                process.Start();
                var result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (result.Contains("Python")) pythonVersion = Convert.ToDouble(result.Split(' ')[1].Remove(3));
                else pythonVersion = -0.0;
            }

            if (pythonVersion.Equals(0.0) || pythonVersion.Equals(-0.0))
            {
                Utils.LogDebug("No python detected", true);
                Utils.LogInfo("Install the latest version of Python to solve this issue", true);
                missingCount++;
            }
            else if (pythonVersion < 3.7)
            {
                Utils.LogDebug("Detected Python version {0}.x - expected 3.7.x or newer", true, pythonVersion);
                Utils.LogInfo("Install the latest version of Python to solve this issue", true);
                missingCount++;
            }

            if (!Directory.Exists(PathMgr.Thirdparty("ljd")))
            {
                Utils.LogDebug("Not found: LuaJIT Raw-Bytecode Decompiler (ljd)", true);
                Utils.LogInfo("Check one of k0np4ku's repository to solve this issue", true);
                missingCount++;
            }

            if (!Directory.Exists(PathMgr.Thirdparty("luajit")))
            {
                Utils.LogDebug("Not found: LuaJIT Just-In-Time Compiler", true);
                Utils.LogInfo("Check one of k0np4ku's repository to solve this issue", true);
                missingCount++;
            }

            if (!Directory.Exists(PathMgr.Thirdparty("unityex")))
            {
                Utils.LogDebug("Not found: UnityEx.exe", true);
                Utils.LogInfo("Check one of k0np4ku's repository to solve this issue", true);
                missingCount++;
            }

            if (missingCount > 0)
            {
                _abort = true;
                Console.WriteLine();
                Utils.Write("Aborted.", true);
            }
        }

        internal static void Main(string[] args)
        {
            // Dependency checker
            Initialize();
            
            if (_abort) return;

            // Check whether arguments are valid by counting the length
            var showHelp = args.Length < 2;

            // Initialize options
            var options = new OptionSet()
            {
                {"u|unlock", "Decrypt Lua", v => SetOption(Options.LuaUnlock)},
                {"l|lock", "Encrypt Lua", v => SetOption(Options.LuaLock)},
                {"d|decompile", "Decompile Lua", v => SetOption(Options.LuaDecompile)},
                {"r|recompile", "Recompile Lua", v => SetOption(Options.LuaRecompile)},
                {"decrypt", "Decrypt AssetBundle", v => SetOption(Options.AssetBundleDecrypt)},
                {"encrypt", "Encrypt AssetBundle", v => SetOption(Options.AssetBundleEncrypt)},
                {"unpack", "Unpack AssetBundle", v => SetOption(Options.AssetBundleUnpack)},
                {"repack", "Repack AssetBundle", v => SetOption(Options.AssetBundleRepack)},
                {"dev1", "Decrypt Lua (Development Mode)", v => SetOption(Options.LuaUnlock, true)},
                {"dev2", "Encrypt Lua (Development Mode)", v => SetOption(Options.LuaLock, true)},
                {"dev3", "Decompile Lua (Development Mode)", v => SetOption(Options.LuaDecompile, true)},
                {"dev4", "Recompile Lua (Development Mode)", v => SetOption(Options.LuaRecompile, true)},
                {"dev5", "Decrypt AssetBundle (Development Mode)", v => SetOption(Options.AssetBundleDecrypt, true)},
                {"dev6", "Encrypt AssetBundle (Development Mode)", v => SetOption(Options.AssetBundleEncrypt, true)},
                {"dev7", "Unpack AssetBundle (Development Mode)", v => SetOption(Options.AssetBundleUnpack, true)},
                {"dev8", "Repack AssetBundle (Development Mode)", v => SetOption(Options.AssetBundleRepack, true)},
                {"<>", v => {
                    if (_currentOption == Options.None) {
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

            // Initialize options
            /* var options = new OptionSet()
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
            };*/

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
                        Utils.Write($@"A file or directory named {value} does not exists.", true);
                    }
                    if (File.Exists(value))
                    {
                        if (OpContains(parameter.Key, "Lua")) ListOfLua.Add(Path.GetFullPath(value));
                        else ListOfAssetBundle.Add(Path.GetFullPath(value));
                    }
                    else if (Directory.Exists(value))
                    {
                        if (OpContains(parameter.Key, "Lua"))
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

            if (OpContains("Lua"))
            {
                foreach (var lua in ListOfLua)
                    LuaMgr.Initialize(lua, OpContains(Options.LuaUnlock) ? Tasks.Decrypt : OpContains(Options.LuaLock) ? Tasks.Encrypt : OpContains(Options.LuaRecompile) ? Tasks.Recompile : Tasks.Decompile);
            }
            else if (OpContains("AssetBundle"))
            {
                foreach (var assetbundle in ListOfAssetBundle)
                    AssetBundleMgr.Initialize(assetbundle, OpContains(Options.AssetBundleDecrypt) ? Tasks.Decrypt : OpContains(Options.AssetBundleEncrypt) ? Tasks.Encrypt : OpContains(Options.AssetBundleUnpack) ? Tasks.Unpack : Tasks.Repack);
            }

            if (Ok && !OpContains(Options.AssetBundleRepack) && !OpContains(Options.AssetBundleDecrypt) && !OpContains(Options.AssetBundleEncrypt))
            {
                Console.WriteLine();
                Utils.Write($"{(OpContains(Options.LuaUnlock) || OpContains(Options.AssetBundleDecrypt) ? "Decrypt" : OpContains(Options.LuaLock) || OpContains(Options.AssetBundleEncrypt) ? "Encrypt" : OpContains(Options.LuaDecompile) ? "Decompile" : OpContains(Options.LuaRecompile) ? "Recompile" : OpContains(Options.AssetBundleUnpack) ? "Unpacking" : "Repacking")} {(OpContains(_currentOption, "Lua") ? string.Empty : "assetbundle ")}is done", true);

                if (!DevelopmentMode && !OpContains(Options.AssetBundleUnpack))
                    Utils.Write("Success: {0} - Failed: {1}", true, LuaMgr.SuccessCount, LuaMgr.FailedCount);
            }
        }

        /// <summary>
        /// Print help message to the terminal
        /// </summary>
        /// <param name="options"></param>
        private static void Help(OptionSet options)
        {
            Utils.Write("Usage: Azurlane.exe <option> <path-to-file(s) or path-to-directory(s)>", true);
            Console.WriteLine();
            Utils.Write("Options:", true);
            options.WriteOptionDescriptions(Console.Out);
        }

        private static bool OpContains(Options option) => _currentOption == option;

        private static bool OpContains(Options option, string key) => option.ToString().Contains(key);

        private static bool OpContains(string key) => _currentOption.ToString().Contains(key);

        //private static bool OpContains(string key1, string key2) => OpContains(key1) && OpContains(key2);

        private static void SetOption(Options option, bool devMode = false)
        {
            _currentOption = option;
            if (devMode)
                DevelopmentMode = true;
        }
    }
}