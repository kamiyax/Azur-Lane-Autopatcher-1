using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Azurlane
{
    public class AssetBundleMgr
    {
        /// <summary>
        /// Decryption and encryption patterns
        /// </summary>
        private static readonly List<byte[]> Decrypt, Encrypt;

        private static readonly object Instance;

        static AssetBundleMgr()
        {
            /* Check whether decryption patterns are initialized properly or null
             * If decryption patterns are null, then initialize and add bytes
             */
            if (Decrypt == null)
            {
                // Initialize
                Decrypt = new List<byte[]>
                {
                    // Add bytes
                    new byte[]
                    {
                        0x55, 0x6E, 0x69, 0x74, 0x79, 0x46, 0x53, 0x00,
                        0x00, 0x00, 0x00, 0x06, 0x35, 0x2E, 0x78, 0x2E
                    }
                };
            }

            /* Check whether encryption patterns are initialized properly or null
             * If encryption patterns are null, then initialize and add bytes
             */
            if (Encrypt == null)
            {
                // Initialize
                Encrypt = new List<byte[]>
                {
                    // Add bytes
                    new byte[]
                    {
                        0xC7, 0xD5, 0xFC, 0x1F, 0x4C, 0x92, 0x94, 0x55,
                        0x85, 0x03, 0x16, 0xA3, 0x7F, 0x7B, 0x8B, 0x55
                    }
                };
            }

            /* Check whether decryption and encryption patterns are null
             * If either one of them are null, then abort
             */
            if (Decrypt == null || Encrypt == null)
                return;

            var assembly = Assembly.Load(Properties.Resources.Salt);
            Instance = Activator.CreateInstance(assembly.GetType("LL.Salt"));
        }

        /// <summary>
        /// [Compare.1] This method is used to compare one bytes with another bytes
        /// </summary>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        internal static bool Compare(byte[] b1, byte[] b2)
        {
            try
            {
                // Iterate through bytes of b2
                for (var i = 0; i < b2.Length; i++)
                {
                    /* Compare b1[i] with b2[i]
                     * If = b1[i] is not the same with b2[i] */
                    if (b1[i] != b2[i])
                        return false; // result is false
                }
            }
            // Catch unexpected errors
            catch (Exception e)
            {
                /* Call exception logger
                 * This method's function is to log unexpected error and write it into a file. */
                Utils.LogException("Exception detected during Compare.1", e);
            }
            return true; // result is true
        }

        /// <summary>
        /// [Compare.2] This method is used to compare one bytes with multiple bytes stored in list through iteration
        /// </summary>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        internal static bool Compare(byte[] b1, List<byte[]> b2)
        {
            try
            {
                // Iterate through list of b2
                foreach (var b in b2)
                {
                    // Iterate through bytes of b
                    for (var i = 0; i < b.Length; i++)
                    {
                        /* Compare b1[i] with b2[i]
                     * If = b1[i] is not the same with b2[i] */
                        if (b1[i] != b[i])
                            return false; // result is false
                    }
                }
            }
            catch (Exception e)
            {
                /* Call exception logger
                 * This method's function is to log unexpected error and write it into a file. */
                Utils.LogException("Exception detected during Compare.2", e);
            }
            return true;  // result is true
        }

        /// <summary>
        /// [Initializer] This method is used to check whether the binary is encrypted, decrypted or invalid/damaged.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="task"></param>
        internal static void Initialize(string path, Tasks task)
        {
            // Read the binary and store the bytes into memory
            var bytes = File.ReadAllBytes(path);

            /* Compare the bytes with encryption patterns
             * If compare result is true, then it means the binary is encrypted */
            if (Compare(bytes, Encrypt))
            {
                // If = our "task" is to "encrypt" the binary
                if (task == Tasks.Encrypt)
                {
                    // Then abort, because the binary has already in encrypted state
                    Utils.LogInfo("AssetBundle is already encrypted... <aborted>");
                    return;
                }
                // Or, if our "task" is to "unpack" or "repack" the binary
                if (task == Tasks.Unpack || task == Tasks.Repack)
                {
                    // Then call the Executor.1 method
                    Execute(bytes, path, Tasks.Decrypt);
                }
                // Then continue if not aborted
            }
            /* Compare the bytes with decryption patterns if the previous result is false
             * If compare result is true, then it means the binary is decrypted */
            else if (Compare(bytes, Decrypt))
            {
                // If = our "task" is to "decrypt" the binary
                if (task == Tasks.Decrypt)
                {
                    // Then abort, because the binary has already in decrypted state
                    Utils.LogInfo("AssetBundle is already decrypted... <aborted>");
                    return;
                }
                // Then continue if not aborted
            }
            /* If the binary is neither encrypted or decrypted
             * It means the binary is invalid/damaged */
            else
            {
                // Abort
                Utils.LogInfo("Not a valid/damaged AssetBundle... <aborted>");
                return;
            }

            // If our "task" is to "decrypt" or "encrypt" the binary
            if (task == Tasks.Decrypt || task == Tasks.Encrypt)
            {
                // Then call the Executor.1 method
                Execute(bytes, path, task);
            }
            // If our "task" is to "unpack" or "repack" the binary
            else if (task == Tasks.Unpack || task == Tasks.Repack)
            {
                // Then call the Executor.2 method
                Execute(path, task);
            }
            // Set the state of program to Ok = true
            Program.Ok = true;
        }

        /// <summary>
        /// [Executor.1] This method is used to encrypt/decrypt the binary
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="path"></param>
        /// <param name="task"></param>
        private static void Execute(byte[] bytes, string path, Tasks task)
        {
            // Send a logInfo to terminal indicating that we're decrypting/encrypting the binary
            Utils.LogInfo("{0} {1}...", task == Tasks.Decrypt ? "Decrypting" : "Encrypting", Path.GetFileName(path));

            var method = Instance.GetType().GetMethod("Make", BindingFlags.Static | BindingFlags.Public);
            bytes = (byte[])method.Invoke(Instance, new object[] { bytes, task == Tasks.Encrypt });

            // Write new (decrypted/encrypted) bytes to "path"
            File.WriteAllBytes(path, bytes);

            // Send a <done> info to terminal indicating that the job is finished
            Utils.WriteLine(" <done>");
        }

        /// <summary>
        /// [Executor.2] This method is used to unpack/repack the binary
        /// </summary>
        /// <param name="path"></param>
        /// <param name="task"></param>
        private static void Execute(string path, Tasks task)
        {
            // Send a logInfo to terminal indicating that we're unpacking/repacking the binary
            Utils.LogInfo("{0} {1}...", task == Tasks.Unpack ? "Unpacking" : "Repacking", Path.GetFileName(path));

            // Run a command
            Utils.Command($"UnityEX.exe {(task == Tasks.Unpack ? "export" : "import")} \"{path}\"");

            // Send a <done> info to terminal indicating that the job is finished
            Utils.WriteLine(" <done>");
        }
    }
}