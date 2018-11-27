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
            var root = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            if (path != null && !File.Exists(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path == null ? root : Path.Combine(root, path);
        }

        internal static string Thirdparty(string path = null) => path != null ? Path.Combine(Local(ConfigMgr.ThirdpartyFolder), path) : Local(ConfigMgr.ThirdpartyFolder);
    }
}