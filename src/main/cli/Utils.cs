using System;
using System.Diagnostics;
using System.IO;

namespace Azurlane
{
    public class Utils
    {
        /// <summary>
        /// Run a command
        /// </summary>
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
        /// Send a debug message to the terminal
        /// </summary>
        /// <param name="message"></param>
        /// /// <param name="writeLine"></param>
        /// <param name="arg"></param>
        internal static void LogDebug(string message, bool writeLine, params object[] arg) => Write($@"[{DateTime.Now:HH:mm:ss}][Debug]> {message}", writeLine, arg);

        internal static void LogException(string message, Exception exception)
        {
            // Send a debug message to terminal
            LogDebug(message, true);

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
        /// Send an info message to the terminal
        /// </summary>
        /// <param name="message"></param>
        /// /// <param name="writeLine"></param>
        /// <param name="arg"></param>
        internal static void LogInfo(string message, bool writeLine, params object[] arg) => Write($@"[{DateTime.Now:HH:mm:ss}][Info]> {message}", writeLine, arg);

        /// <summary>
        /// A better version of Console.WriteLine
        /// </summary>
        /// <param name="message"></param>
        /// <param name="writeLine"></param>
        /// <param name="arg"></param>
        internal static void Write(string message, bool writeLine, params object[] arg)
        {
            Console.Write(message, arg);
            if (writeLine)
                Console.WriteLine();
        }
    }
}