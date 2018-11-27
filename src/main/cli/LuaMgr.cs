using System;
using System.IO;
using System.Text;

namespace Azurlane
{
    internal static class LuaMgr
    {
        internal static int SuccessCount, FailedCount;

        internal enum State
        {
            None,
            Encrypted,
            Decrypted
        }

        /// <summary>
        /// [Initializer] This method is used to check whether the binary is encrypted, decrypted or invalid/damaged
        /// </summary>
        /// <param name="lua"></param>
        /// <param name="task"></param>
        internal static void Initialize(string lua, Tasks task)
        {
            // Read the binary and store the bytes into memory
            var bytes = File.ReadAllBytes(lua);

            // Let's declare our binary's state as none
            var state = State.None;

            /* If (3rd byte of bytes) is 0x80
             * If true, then the binary is encrypted
             */
            if (bytes[3] == 0x80)
            {
                // Let's change the state to encrypted
                state = State.Encrypted;

                // If our "task" is to "encrypt" the binary
                if (task == Tasks.Encrypt)
                {
                    // Then abort, because the binary has already in encrypted state
                    Utils.LogInfo("{0} is already encrypted... <aborted>", true, Path.GetFileName(lua).Replace(".txt", string.Empty));
                    return;
                }
                // Or, if our "task" is to "decompile" the binary
                if (task == Tasks.Decompile)
                {
                    // Then call the Executor.2 method
                    Execute(lua, bytes, Tasks.Decrypt, state);
                }
                // Then continue if not aborted
            }
            /* If (3rd byte of bytes) is 0x02
             * If true, then the binary is decrypted */
            else if (bytes[3] == 0x02)
            {
                // Let's change the state to decrypted
                state = State.Decrypted;

                // If our "task" is to "decrypt" the binary
                if (task == Tasks.Decrypt)
                {
                    // Then abort, because the binary has already in decrypted state
                    Utils.LogInfo("{0} is already decrypted... <aborted>", true, Path.GetFileName(lua).Replace(".txt", string.Empty));
                    return;
                }
                // Then continue if not aborted
            }
            /* If the binary is neither encrypted or decrypted
             * As long our task is not to "recompile", it means the binary is invalid/damaged */
            else if (task != Tasks.Recompile)
            {
                // Abort
                Utils.LogInfo("Not a valid or damaged lua file... <aborted>", true);
                return;
            }

            // If our "task" is to "decrypt" or "encrypt" the binary
            if (task == Tasks.Decrypt || task == Tasks.Encrypt)
            {
                // Then call the Executor.1 method
                Execute(lua, bytes, task, state);
            }
            // If our "task" is to "decompile" or "recompile" the binary
            else if (task == Tasks.Decompile || task == Tasks.Recompile)
            {
                // Then call the Executor.2 method
                Execute(lua, task);
            }
            // Set the state of program to Ok = true
            Program.Ok = true;
        }

        /// <summary>
        /// [Executor.1] This method is used to decrypt/encrypt the binary
        /// </summary>
        /// <param name="lua"></param>
        /// <param name="bytes"></param>
        /// <param name="task"></param>
        /// <param name="state"></param>
        private static void Execute(string lua, byte[] bytes, Tasks task, State state)
        {
            if (!Program.DevelopmentMode)
            {
                lua = Path.Combine(PathMgr.Local(task == Tasks.Decrypt ? "decrypted_lua" : "encrypted_lua"), Path.GetFileName(lua));

                if (File.Exists(lua))
                    File.Delete(lua);
            }

            try
            {
                // Send a logInfo to terminal indicating that we're decrypting/encrypting the binary
                Utils.LogInfo("{0} {1}...", false, task == Tasks.Decrypt ? "Decrypting" : "Encrypting", Path.GetFileName(lua).Replace(".txt", string.Empty));
                using (var stream = new MemoryStream(bytes))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        // src: ljd\rawdump\header.py + Perfare
                        var magic = reader.ReadBytes(3);
                        var version = reader.ReadByte();
                        var bits = reader.ReadUleb128();

                        var is_stripped = ((bits & 2u) != 0u);
                        if (!is_stripped)
                        {
                            var length = reader.ReadUleb128();
                            var name = Encoding.UTF8.GetString(reader.ReadBytes((int)length));
                        }

                        while (reader.BaseStream.Position < reader.BaseStream.Length)
                        {
                            var size = reader.ReadUleb128();

                            if (size == 0)
                                break;

                            var next = reader.BaseStream.Position + size;
                            bits = reader.ReadByte();

                            var arguments_count = reader.ReadByte();
                            var framesize = reader.ReadByte();
                            var upvalues_count = reader.ReadByte();
                            var complex_constants_count = reader.ReadUleb128();
                            var numeric_constants_count = reader.ReadUleb128();
                            var instructions_count = reader.ReadUleb128();

                            var start = (int)reader.BaseStream.Position;

                            // If the "state" of the binary is encrypted and our "task" is to "decrypt"
                            if (state == State.Encrypted && task == Tasks.Decrypt)
                            {
                                bytes[3] = 0x02;
                                bytes = Unlock(start, bytes, (int)instructions_count);
                            }
                            // If the "state" of the binary is decrypted and our "task" is to "encrypt"
                            else if (state == State.Decrypted && task == Tasks.Encrypt)
                            {
                                bytes[3] = 0x80;
                                bytes = Lock(start, bytes, (int)instructions_count);
                            }
                            // Abort if neither of them
                            else break;

                            reader.BaseStream.Position = next;
                        }
                    }
                }
                File.WriteAllBytes(lua, bytes);
            }
            catch (Exception e)
            {
                /* Call exception logger
                 * This method's function is to log unexpected error and write it into a file. */
                Utils.LogException($"Exception detected (Executor.1) during {(task == Tasks.Decrypt ? "decrypting" : "encrypting")} {Path.GetFileName(lua).Replace(".txt", string.Empty)}", e);
            }
            finally
            {
                if (File.Exists(lua))
                {
                    SuccessCount++;
                    Utils.Write(" <done>", true);
                }
                else
                {
                    FailedCount++;
                    Utils.Write(" <failed>", true);
                }
            }
        }

        /// <summary>
        /// [Executor.2] This method is used to decompile/recompile the binary
        /// </summary>
        /// <param name="lua"></param>
        /// <param name="task"></param>
        private static void Execute(string lua, Tasks task)
        {
            var luaPath = lua;
            if (!Program.DevelopmentMode)
            {
                if (lua.ToLower().Contains("unity_assets_files"))
                    lua = Path.Combine(PathMgr.Local("decrypted_lua"), Path.GetFileName(lua));

                luaPath = Path.Combine(PathMgr.Local(task == Tasks.Decompile ? "decompiled_lua" : "recompiled_lua"), Path.GetFileName(lua));
            }

            Utils.LogInfo("{0} {1}...", false, task == Tasks.Decompile ? "Decompiling" : "Recompiling", Path.GetFileName(lua).Replace(".txt", string.Empty));
            try
            {
                Utils.Command(task == Tasks.Decompile ? $"python main.py -f \"{lua}\" -o \"{luaPath}\"" : $"luajit.exe -b \"{lua}\" \"{luaPath}\"");
            }
            catch (Exception e)
            {
                Utils.LogException($"Exception detected (Executor.2) during {(task == Tasks.Decompile ? "decompiling" : "recompiling")} {Path.GetFileName(lua).Replace(".txt", string.Empty)}", e);
            }
            finally
            {
                if (File.Exists(lua))
                {
                    SuccessCount++;
                    Utils.Write(" <done>", true);
                }
                else
                {
                    FailedCount++;
                    Utils.Write(" <failed>", true);
                }
            }
        }

        private static byte[] Lock(int start, byte[] bytes, int count)
        {
            var result = start;
            result += 4;
            var v2 = 0;
            do
            {
                var v3 = bytes[result - 4];
                result += 4;
                var v4 = bytes[result - 7] ^ v2++;
                bytes[result - 8] = (byte)(Properties.Resources.Lock[v3] ^ v4);
            }
            while (v2 != count);
            return bytes;
        }

        private static uint ReadUleb128(this BinaryReader reader)
        {
            // ljd\util\binstream.py + Perfare
            uint value = reader.ReadByte();
            if (value >= 0x80)
            {
                var bitshift = 0;
                value &= 0x7f;
                while (true)
                {
                    var b = reader.ReadByte();
                    bitshift += 7;
                    value |= (uint)((b & 0x7f) << bitshift);
                    if (b < 0x80)
                        break;
                }
            }
            return value;
        }

        private static byte[] Unlock(int start, byte[] bytes, int count)
        {
            var result = start;
            result += 4;
            var v2 = 0;
            do
            {
                var v3 = bytes[result - 4];
                result += 4;
                var v4 = bytes[result - 7] ^ v3 ^ (v2++ & 0xFF);
                bytes[result - 8] = Properties.Resources.Unlock[v4];
            }
            while (v2 != count);
            return bytes;
        }
    }
}