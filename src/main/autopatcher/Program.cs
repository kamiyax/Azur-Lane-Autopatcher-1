using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Azurlane
{
    internal enum Mods
    {
        GodMode,
        WeakEnemy,
        GodMode_Damage,
        GodMode_Cooldown,
        GodMode_WeakEnemy,
        GodMode_Damage_Cooldown,
        GodMode_Damage_WeakEnemy,
        GodMode_Damage_Cooldown_WeakEnemy
    }

    internal static class Program
    {
        private static bool _abort;

        internal static List<string> ListOfLua;
        internal static Dictionary<Mods, bool> ListOfMod;

        internal static void SetValue(Mods key, bool value) => ListOfMod[key] = value;

        private static void AddLua(string value) => ListOfLua.Add(value);

        private static bool GetValue(Mods key) => ListOfMod[key];

        private static void CheckVersion()
        {
            try
            {
                using (var wc = new System.Net.WebClient())
                {
                    var latestStatus = wc.DownloadString(Properties.Resources.autopatcherStatus);
                    if (latestStatus != "ok")
                    {
                        _abort = true;
                        return;
                    }

                    var latestVersion = wc.DownloadString(Properties.Resources.autopatcherVersion);
                    if ((string)ConfigMgr.GetValue(ConfigMgr.Key.Version) != latestVersion)
                    {
                        Utils.Write("[Obsolete CLI version]", true);
                        Utils.Write("Download the latest version from:", true);
                        Utils.Write("github.com/k0np4ku/Azur-Lane-Autopatcher", true);
                        _abort = true;
                    }
                }
            }
            catch
            {
                _abort = true;
            }
        }

        private static void Initialize()
        {
            if (ListOfMod == null)
                ListOfMod = new Dictionary<Mods, bool>();

            foreach (Mods mod in Enum.GetValues(typeof(Mods)))
                ListOfMod.Add(mod, false);

            if (ListOfLua == null)
                ListOfLua = new List<string>();

            ConfigMgr.Initialize();
            CheckVersion();

            var missingCount = 0;
            var pythonVersion = 0.0;

            try
            {
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

                    if (result.Contains("Python"))
                        pythonVersion = Convert.ToDouble(result.Split(' ')[1].Remove(3));
                    else pythonVersion = -0.0;
                }
            }
            catch
            {
                // Empty
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
                Utils.LogInfo("Refer to one of k0np4ku's repository to solve this issue", true);
                missingCount++;
            }

            if (!Directory.Exists(PathMgr.Thirdparty("luajit")))
            {
                Utils.LogDebug("Not found: LuaJIT Just-In-Time Compiler", true);
                Utils.LogInfo("Refer to one of k0np4ku's repository to solve this issue", true);
                missingCount++;
            }

            if (!Directory.Exists(PathMgr.Thirdparty("unityex")))
            {
                Utils.LogDebug("Not found: UnityEx.exe", true);
                Utils.LogInfo("Refer to one of k0np4ku's repository to solve this issue", true);
                missingCount++;
            }

            if (missingCount > 0)
                _abort = true;

            AddLua("aircraft_template.lua.txt");
            AddLua("enemy_data_statistics.lua.txt");

            if (GetValue(Mods.GodMode_Damage) || GetValue(Mods.GodMode_Cooldown) || GetValue(Mods.GodMode_Damage_Cooldown) ||
                GetValue(Mods.GodMode_Damage_WeakEnemy) || GetValue(Mods.GodMode_Damage_Cooldown_WeakEnemy))
            {
                AddLua("weapon_property.lua.txt");
            }
        }

        [STAThread]
        private static void Main(string[] args)
        {
            Initialize();
            if (_abort)
                return;

            foreach (var a in ListOfMod)
            {
                Console.WriteLine("key: {0} - value: {1}", a.Key, a.Value);
            }

            foreach (var a in ConfigMgr.Instance)
            {
                Console.WriteLine("key: {0} - value: {1}", a.Key, a.Value);
            }

            foreach (var a in ListOfLua)
            {
                Console.WriteLine(a);
            }
        }
    }
}