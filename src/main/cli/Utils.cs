using System;
using System.Diagnostics;
using System.IO;

namespace Azurlane
{
    /// <summary>
    /// Utils
    /// Sometimes we want to simplify things
    /// You can put it here
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// Command
        /// </summary>
        /// <param name="argument"></param>
        internal static void Command(string argument)
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = "cmd";
                process.StartInfo.Arguments = $"/c {argument}";
                process.StartInfo.WorkingDirectory = PathMgr.Thirdparty();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                process.WaitForExit();
            }
        }

        /// <summary>
        /// LogDebug
        /// Send a message to the terminal with [Debug] prefix
        /// Use this method for anything related to logging a debug (errors, etc)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="arg"></param>
        internal static void LogDebug(string message, params object[] arg) => WriteLine($@"[{DateTime.Now:HH:mm:ss}][Debug]> {message}", arg);

        internal static void LogException(string message, Exception exception)
        {
            // Send a debug message to terminal
            LogDebug(message);

            // Checking whether Logs.txt is exists in local folder
            if (!File.Exists(PathMgr.Local("Logs.txt")))
                // Create an empty Logs.txt if file not exists
                File.WriteAllText(PathMgr.Local("Logs.txt"), string.Empty);

            using (var streamWriter = new StreamWriter(PathMgr.Local("Logs.txt"), true))
            {
                streamWriter.WriteLine("=== START ==============================================================================");
                streamWriter.WriteLine(message);
                streamWriter.WriteLine($"Date: {DateTime.Now.ToString()}");
                streamWriter.WriteLine($"Exception Message: {exception.Message}");
                streamWriter.WriteLine($"Exception StackTrace: {exception.StackTrace}");
                streamWriter.WriteLine("=== END ================================================================================");
                streamWriter.WriteLine();
            }
        }

        /// <summary>
        /// LogInfo
        /// Send a message to the terminal with [Info] prefix
        /// Use this method for anything related to logging an information (status, etc)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="arg"></param>
        internal static void LogInfo(string message, params object[] arg) => WriteLine($@"[{DateTime.Now:HH:mm:ss}][Info]> {message}", arg);

        /// <summary>
        /// WriteLine
        /// We're using this instead of Console.WriteLine for the sake of convenience
        /// </summary>
        /// <param name="message"></param>
        /// <param name="arg"></param>
        internal static void WriteLine(string message, params object[] arg)
        {
            Console.Write(message, arg);
            Console.WriteLine();
        }
    }
}