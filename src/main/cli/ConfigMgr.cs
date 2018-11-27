using System.Collections.Generic;
using System.IO;

namespace Azurlane
{
    public class ConfigMgr
    {
        /// <summary>
        /// Location of third party folder which retrieved from configuration.ini
        /// </summary>
        internal static string ThirdpartyFolder;

        private static readonly Dictionary<string, string> Instance;

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
        /// [Initializer] This method is used to retrieve the location of 3rdparty folder
        /// </summary>
        internal static void Initialize() => ThirdpartyFolder = GetString("3rdparty_folder");

        /// <summary>
        /// This method is used to retrieve value of key of dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string GetString(string key) => Instance[key];
    }
}