using System.IO;
using System.Reflection;

namespace Azurlane
{
    internal static class PathMgr
    {
        internal static string Local(string path = null)
        {
            // Get the root directory of binary and store it to variable
            var root = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            // Check whether argument "path" is not null and whether file and directory is exists
            if (path != null && !File.Exists(path) && !Directory.Exists(path))
                // If meet the condition, then create a directory
                Directory.CreateDirectory(path);

            // return root variable if path is null, otherwise return combined path
            return path == null ? root : Path.Combine(root, path);
        }

        internal static string Lua(string name) => Path.Combine(Temp("Unity_Assets_Files"), $"{name}\\CAB-android");

        internal static string Lua(string name, string lua) => Path.Combine(Lua(name), lua);

        internal static string Temp(string path = null) => path != null ? Path.Combine(Local((string)ConfigMgr.GetValue(ConfigMgr.Key.Temporary_Folder)), path) : Local((string)ConfigMgr.GetValue(ConfigMgr.Key.Temporary_Folder));

        internal static string Thirdparty(string path = null) => path != null ? Path.Combine(Local((string)ConfigMgr.GetValue(ConfigMgr.Key.Thirdparty_Folder)), path) : Local((string)ConfigMgr.GetValue(ConfigMgr.Key.Thirdparty_Folder));
    }
}