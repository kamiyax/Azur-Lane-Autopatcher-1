using System.Collections.Generic;
using System.IO;

namespace Azurlane
{
    /// <summary>
    /// ConfigMgr
    /// Anything related to working with configuration
    /// </summary>
    public class ConfigMgr
    {
        /// <summary>
        /// ThirdparyFolder
        /// </summary>
        internal static string ThirdpartyFolder;

        /// <summary>
        /// Instance
        /// </summary>
        private static readonly Dictionary<string, string> Instance;

        /// <summary>
        /// Static constructor
        /// </summary>
        static ConfigMgr()
        {
            // We make sure whether the instance of ConfigMgr is null
            if (Instance == null)
            {
                // Store the path of config file into variable, for the sake of convenience
                var iniPath = PathMgr.Local("Configuration.ini");

                // Initialize instance
                Instance = new Dictionary<string, string>();

                // Iterate through each line of config file
                foreach (var line in File.ReadAllLines(iniPath))
                {
                    // Check whether the line contains '=' character
                    if (line.Contains("="))
                    {
                        // If true, then split the line
                        var s = line.Split('=');

                        // Check whether the first string which is the key, is named "3rdparty_folder"
                        if (s[0] == "3rdparty_folder")
                            // If true, then add both key (s[0]) and value (s[1]) to dictionary
                            Instance.Add(s[0], s[1]);
                    }
                }
            }
        }

        /// <summary>
        /// Initialize
        /// Get value of key "3rdparty_folder" and store it to "ThirdpartyFolder"
        /// </summary>
        internal static void Initialize() => ThirdpartyFolder = GetString("3rdparty_folder");

        /// <summary>
        /// GetString
        /// for the sake of convenience
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string GetString(string key) => Instance[key];
    }
}