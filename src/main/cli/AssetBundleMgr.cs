using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Azurlane
{
    public class AssetBundleMgr
    {
        /// <summary>
        /// Decryption and Encryption patterns
        /// </summary>
        private static readonly List<byte[]> DecryptionPatterns, EncryptionPatterns;

        private static readonly object Instance;

        static AssetBundleMgr()
        {
            // Check whether decryption patterns are null
            if (DecryptionPatterns == null)
            {
                // Initialize
                DecryptionPatterns = new List<byte[]>
                {
                    new byte[]
                    {
                        0x55, 0x6E, 0x69, 0x74, 0x79, 0x46, 0x53, 0x00,
                        0x00, 0x00, 0x00, 0x06, 0x35, 0x2E, 0x78, 0x2E
                    }
                };
            }

            // Check whether encryption patterns are null
            if (EncryptionPatterns == null)
            {
                // Initialize
                EncryptionPatterns = new List<byte[]>
                {
                    new byte[]
                    {
                        0xC7, 0xD5, 0xFC, 0x1F, 0x4C, 0x92, 0x94, 0x55,
                        0x85, 0x03, 0x16, 0xA3, 0x7F, 0x7B, 0x8B, 0x55
                    }
                };
            }

            /* Check whether decryption and encryption patterns are null
             * And abort if either one of them are null */
            if (DecryptionPatterns == null || EncryptionPatterns == null)
                return;

            var assembly = Assembly.Load(Properties.Resources.Salt);
            Instance = Activator.CreateInstance(assembly.GetType("LL.Salt"));
        }

        /// <summary>
        /// [Compare.1] Compare bytes
        /// </summary>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        internal static bool Compare(byte[] b1, byte[] b2)
        {
            try
            {
                for (var i = 0; i < b2.Length; i++)
                {
                    if (b1[i] != b2[i])
                        return false;
                }
            }
            catch (Exception e)
            {
                Utils.LogException("Exception detected during Compare.1", e);
            }
            return true;
        }

        /// <summary>
        /// [Compare.2] Compare bytes
        /// </summary>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        internal static bool Compare(byte[] b1, List<byte[]> b2)
        {
            try
            {
                foreach (var b in b2)
                {
                    for (var i = 0; i < b.Length; i++)
                    {
                        if (b1[i] != b[i])
                            return false;
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException("Exception detected during Compare.2", e);
            }
            return true;
        }

        /// <summary>
        /// [Initializer] Processing a binary and check whether the binary is encrypted, decrypted, damaged or invalid
        /// </summary>
        /// <param name="path"></param>
        /// <param name="task"></param>
        internal static void Initialize(string path, Tasks task)
        {
            // Read all bytes of a binary and store them into memory
            var bytes = File.ReadAllBytes(path);
        
            // Compare bytes of the binary with encryption patterns
            if (Compare(bytes, EncryptionPatterns))
            {
                // Check whether our task is to encrypt the binary
                if (task == Tasks.Encrypt)
                {
                    // Abort because the binary is already encrypted
                    Utils.LogInfo("AssetBundle is already encrypted... <aborted>", true);
                    return;
                }
                // Check whether our task is to unpack or repack the binary
                if (task == Tasks.Unpack || task == Tasks.Repack)
                {
                    // Call Executor.1 (Decrypt the binary before processing it)
                    Execute(bytes, path, Tasks.Decrypt);
                }
            }
            // Compare bytes of a binary with decryption patterns
            else if (Compare(bytes, DecryptionPatterns))
            {
                // Check whether our task is to decrypt the binary
                if (task == Tasks.Decrypt)
                {
                    // Abort because the binary is already decrypted
                    Utils.LogInfo("AssetBundle is already decrypted... <aborted>", true);
                    return;
                }
            }
            else
            {
                // Abort because the binary is valid or damaged
                Utils.LogInfo("Not a valid/damaged AssetBundle... <aborted>", true);
                return;
            }
            
            // Check whether our task is to decrypt or encrypt the binary
            if (task == Tasks.Decrypt || task == Tasks.Encrypt)
            {
                // Call Executor.1 (decryption/encryption)
                Execute(bytes, path, task);
            }
            // Check whether our task is to unpack or repack the binary
            else if (task == Tasks.Unpack || task == Tasks.Repack)
            {
                // Call Executor.1 (unpacker/repacker)
                Execute(path, task);
            }
            // Set the state of the program to true
            Program.Ok = true;
        }

        /// <summary>
        /// [Executor.1] Encrypt or decrypt a binary
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="path"></param>
        /// <param name="task"></param>
        private static void Execute(byte[] bytes, string path, Tasks task)
        {
            // Send a logInfo to the terminal
            Utils.LogInfo("{0} {1}...", false, task == Tasks.Decrypt ? "Decrypting" : "Encrypting", Path.GetFileName(path));

            var method = Instance.GetType().GetMethod("Make", BindingFlags.Static | BindingFlags.Public);
            bytes = (byte[])method.Invoke(Instance, new object[] { bytes, task == Tasks.Encrypt });
            
            // Write the decrypted/encrypted bytes to the designated location
            File.WriteAllBytes(path, bytes);
            
            // Send a done message
            Utils.Write(" <done>", true);
        }

        /// <summary>
        /// [Executor.1] Unpack or repack a binary
        /// </summary>
        /// <param name="path"></param>
        /// <param name="task"></param>
        private static void Execute(string path, Tasks task)
        {
            // Send a logInfo to the terminal
            Utils.LogInfo("{0} {1}...", false, task == Tasks.Unpack ? "Unpacking" : "Repacking", Path.GetFileName(path));

            // Run a new command
            Utils.Command($"UnityEX.exe {(task == Tasks.Unpack ? "export" : "import")} \"{path}\"", PathMgr.Thirdparty("unityex"));

            // Send a done message
            Utils.Write(" <done>", true);
        }
    }
}