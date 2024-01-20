using ShellProgressBar;

namespace PARCrypt
{
    internal class Crypto
    {
        internal static void Decrypt(List<byte[]> data, byte[] key = null)
        {
            Console.WriteLine("\nDecrypting...");
            byte[] encMagic = data[0].Take(4).ToArray();
            if (key == null) key = GetKey(encMagic);

            XOR(data, key);
            Rotate(data);
        }


        internal static void Encrypt(List<byte[]> data, byte[] key)
        {
            Console.WriteLine("\nEncrypting...");
            Rotate(data, false);
            XOR(data, key);
        }


        internal static void XOR(List<byte[]> data, byte[] key)
        {
            uint keyLength = (uint)key.Length;
            ulong fileIterator = 0;

            Console.WriteLine("\n");
            ProgressBar pbar = new ProgressBar(data.Count, "Performing XOR...");

            foreach (byte[] chunk in data)
            {
                int chunkLength = chunk.Length;
                for (int chunkIterator = 0;  chunkIterator < chunkLength; fileIterator++, chunkIterator++)
                {
                    chunk[chunkIterator] = (byte)(chunk[chunkIterator] ^ key[fileIterator % keyLength]);
                }

                pbar.Tick();
            }

            pbar.Dispose();
        }


        internal static void Rotate(List<byte[]> data, bool left = true)
        {
            Console.WriteLine("\n");
            ProgressBar pbar = new ProgressBar(data.Count, "Rotating bits...");

            foreach (byte[] chunk in data)
            {
                for (int i = 0; i < chunk.Length / 8; i++)
                {
                    ulong value = BitConverter.ToUInt64(chunk, i * 8);

                    if (left)
                        value = (value << i) | (value >> (64 - i));
                    else
                        value = (value >> i) | (value << (64 - i));

                    byte[] bytes = BitConverter.GetBytes(value);
                    Array.Copy(bytes, 0, chunk, i * 8, 8);
                }

                pbar.Tick();
            }

            pbar.Dispose();
        }


        internal static byte[] GetKey(byte[] encMagic)
        {
            string hexStr = BitConverter.ToString(encMagic).Replace("-", "").ToUpper();
            byte[] key = Util.GetEmbeddedKeyFile(hexStr);
            if (key == null) throw new Exception("No valid key found.");
            return key;
        }
    }
}
