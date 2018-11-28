using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Azurlane
{
    internal static class Program
    {
        internal static List<string> ListOfLua, ListOfMod;

        private static bool _abort;

        private static void Initialize()
        {
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
            {
                _abort = true;
                Console.WriteLine();
                Utils.Write("Aborted.", true);
            }
        }

        [STAThread]
        private static void Main(string[] args)
        {
            // Dependency checker
            Initialize();

            if (_abort) return;

            if (args.Length < 1)
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Title = @"Open an AssetBundle...";
                    dialog.Filter = @"AssetBundle|scripts*";
                    dialog.CheckFileExists = true;
                    dialog.Multiselect = false;
                    dialog.ShowDialog();

                    if (File.Exists(dialog.FileName))
                    {
                        args = new[] { dialog.FileName };
                    }
                    else
                    {
                        Utils.Write("Please open an AssetBundle...", true);
                    }
                }
            }
            else if (args.Length > 1)
            {
                Utils.Write("Invalid argument, usage: Azurlane.exe <path-to-scripts>", true);
            }
        }
    }
}