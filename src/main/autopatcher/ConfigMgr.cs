using System.Collections.Generic;

namespace Azurlane
{
    internal static class ConfigMgr
    {
        private static Dictionary<string, string> Instance;

        static ConfigMgr()
        {
            if (Program.ListOfLua == null)
            {
                Program.ListOfLua = new List<string>()
                {
                    "aircraft_template.lua.txt",
                    "enemy_data_statistics.lua.txt"
                };
            }
        }
    }
}