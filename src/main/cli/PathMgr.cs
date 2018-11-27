using System.IO;
using System.Reflection;

namespace Azurlane
{
    /// <summary>
    /// PathMgr
    /// Anything related to Path/Location
    /// </summary>
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

        internal static string Thirdparty(string path = null) => path != null ? Path.Combine(Local(ConfigMgr.ThirdpartyFolder), path) : Local(ConfigMgr.ThirdpartyFolder);
    }
}