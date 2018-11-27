using System;
using System.Collections.Generic;

namespace Azurlane
{
    internal static class Program
    {
        private static Dictionary<string, List<string>> Parameters = new Dictionary<string, List<string>>();
        private static List<string> ListOfAssetBundle = new List<string>(), ListOfLua = new List<string>();

        internal static void Main(string[] args)
        {
            var _showHelp = args.Length < 2;

            if (_showHelp)
            {
                return;
            }
            else
            {
                try
                {
                }
                catch (Exception e)
                {
                }
            }
        }
    }
}