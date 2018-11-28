using System.Collections.Generic;
using System.IO;

namespace Azurlane
{
    public class ConfigMgr
    {
        internal static string Version = "2018.11.28.01";
        internal static string ThirdpartyFolder;
        private static readonly Dictionary<string, string> Instance;

        static ConfigMgr()
        {
            // Check whether the instance is null
            if (Instance == null)
            {
                // Store location of configuration.ini into variable
                var iniPath = PathMgr.Local("Configuration.ini");

                // Initialize
                Instance = new Dictionary<string, string>();

                // Iterate through each line of configuration.ini
                foreach (var line in File.ReadAllLines(iniPath))
                {
                    // Check whether line contains '=' character
                    if (line.Contains("="))
                    {
                        // Split the line
                        var s = line.Split('=');

                        // Check whether key is "3rdparty_folder"
                        if (s[0] == "3rdparty_folder")
                            // Add key and value to dictionary
                            Instance.Add(s[0], s[1]);
                    }
                }
            }
        }

        internal static void Initialize() => ThirdpartyFolder = GetString("3rdparty_folder");

        private static string GetString(string key) => Instance[key];
    }
}