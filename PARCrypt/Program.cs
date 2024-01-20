using CommandLine;
using Sharprompt;
using System.Reflection;

namespace PARCrypt
{
    internal class PARCrypt
    {
        class Options
        {
            [Value(0, MetaName = "file", Required = true, HelpText = "Input file.")]
            public string InputFile { get; set; }

            [Option('o', "out", Required = false, HelpText = "Output file.")]
            public string? OutputFile { get; set; }

            [Option('d', "decrypt", Default = true, HelpText = "Decrypt the input file.")]
            public bool Decrypt { get; set; }

            [Option('e', "encrypt", Default = false, HelpText = "Encrypt the input file.")]
            public bool Encrypt { get; set; }

            [Option('k', "key", Required = false, HelpText = "Key file.")]
            public string? Key { get; set; }
        }


        const int CHUNK_SIZE = 128 * 1024 * 1024; //128MB


        static void Main(string[] args)
        {
            ShowTitle();

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    byte[]? key = null;
                    if (o.Key != null) key = File.ReadAllBytes(o.Key);

                    if (o.Encrypt)
                    {
                        string outPath = $"{Path.Combine(Path.GetDirectoryName(o.InputFile), Path.GetFileNameWithoutExtension(o.InputFile))}.encrypted.par";
                        if (o.OutputFile != null) outPath = o.OutputFile;

                        List<byte[]> fileData = ReadFile(o.InputFile);

                        if (o.Key == null)
                        {
                            Crypto.Encrypt(fileData, SelectKey());
                        }
                        else
                        {
                            Crypto.Encrypt(fileData, key);
                        }

                        WriteFile(fileData, outPath);

                        return;
                    }

                    if (o.Decrypt)
                    {
                        string outPath = $"{Path.Combine(Path.GetDirectoryName(o.InputFile), Path.GetFileNameWithoutExtension(o.InputFile))}.decrypted.par";
                        if (o.OutputFile != null) outPath = o.OutputFile;

                        long fileSize = new FileInfo(o.InputFile).Length;
                        List<byte[]> fileData = ReadFile(o.InputFile);

                        Crypto.Decrypt(fileData, key);

                        WriteFile(fileData, outPath);

                        return;
                    }
                });
        }


        private static byte[] SelectKey()
        {
            string keyValue = Prompt.Select("Select the key to use", Info.KeyInfo.Values.ToArray());
            string keyName = Info.KeyInfo.FirstOrDefault(x => x.Value == keyValue).Key;

            return Util.GetEmbeddedKeyFile(keyName);
        }


        private static List<byte[]> ReadFile(string path)
        {
            Console.WriteLine("\nReading file...");

            List<byte[]> fileData = new List<byte[]>();
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[CHUNK_SIZE];
                int bytesRead;

                while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    byte[] chunk = new byte[bytesRead];
                    Array.Copy(buffer, chunk, bytesRead);
                    fileData.Add(chunk);
                }
            }

            return fileData;
        }


        private static void WriteFile(List<byte[]> fileData, string path)
        {
            Console.WriteLine("\nWriting file...");

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                foreach (byte[] chunk in fileData)
                {
                    fs.Write(chunk, 0, chunk.Length);
                }
            }
        }


        private static void ShowTitle()
        {
            string titleStr = "";
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("title.txt"));
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                titleStr = reader.ReadToEnd();
            }
            titleStr = titleStr.Replace("0.0.0.0", assembly.GetName().Version.ToString());
            Console.WriteLine(titleStr);
        }
    }
}