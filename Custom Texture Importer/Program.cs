using CUE4Parse;
using Custom_Texture_Importer;
using Custom_Texture_Importer.Compression;
using DiscordRPC;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Custom_Texture_Importer;

public static class Program
{
    public static async Task Main()
    {
        DiscordRpcClient client = new("958805762455523358");
        client.Initialize();
        client.SetPresence(new RichPresence()
        {
            Details = "Made by @owenonhxd",
            State = "Importing a texture.",
            Assets = new Assets()
            {
                LargeImageKey = "54129bd57b2f996b25c6759b9833f1e9",
                LargeImageText = "Custom Texture Importer (Made by Owen)",
            }
        });

        const string config = "config.json";
        if (!File.Exists(config)) File.WriteAllText(config, JsonConvert.SerializeObject(FortniteUtil.ConfigData));
        else FortniteUtil.ConfigData = JsonConvert.DeserializeObject<Config>(File.ReadAllText(config));

        var provider = new Provider()._provider;
        while (true)
        {
            Console.Write("Input the path to the texture's ubulk to replace > ");
            var texturePath = Console.ReadLine().Replace(".uasset", ".ubulk");
            if (texturePath is null ||
                texturePath == string.Empty)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                WriteLineColored(ConsoleColor.Red, "Please input a path");
                Console.ResetColor();
                continue;
            }
            Owen.IsExporting = true;
            provider.SaveAsset(texturePath);
            Console.Write("Input your custom ubulk (must be the same size) > ");
            var ubulkBytes = File.ReadAllBytes(Console.ReadLine().Replace("\"", string.Empty));
            var chunked = ChunkData(ubulkBytes);

            await Backup.BackupFile(Owen.Path);

            var bw = new BinaryWriter(File.OpenWrite(Owen.Partition == 0 ? Owen.Path.Replace("WindowsClient", FortniteUtil.ConfigData.BackupFileName).Replace(".utoc", ".ucas") : Owen.Path.Replace("WindowsClient", FortniteUtil.ConfigData.BackupFileName + "_s" + Owen.Partition).Replace(".utoc", ".ucas")));
            var mem = new MemoryStream(File.Exists(Owen.Path.Replace("WindowsClient", FortniteUtil.ConfigData.BackupFileName)) ? File.ReadAllBytes(Owen.Path.Replace("WindowsClient", FortniteUtil.ConfigData.BackupFileName)) : File.ReadAllBytes(Owen.Path));
            long tocOffset = Provider.IndexOfSequence(mem.ToArray(), BitConverter.GetBytes((int)Owen.Offsets[0]));
            long written = 0;

            Console.WriteLine();
            WriteLineColored(ConsoleColor.Green, "Processing...");
            var progress = new ProgressBar();
            for (int i = 0; i < chunked.Count; i++)
            {
                var compressed = Oodle.Compress(chunked[i]);
                bw.BaseStream.Position = Owen.Offsets[0] + written;
                var longAsBytes = BitConverter.GetBytes((int)bw.BaseStream.Position);
                bw.Write(compressed, 0, compressed.Length);
                written += compressed.Length + 10;
                mem.Position = tocOffset + i * 12;
                mem.Write(longAsBytes, 0, longAsBytes.Length);
                mem.Position += 1;
                var intAsBytes = BitConverter.GetBytes((ushort)compressed.Length);
                mem.Write(intAsBytes, 0, intAsBytes.Length);
                //  mem.Position += 1;
                //  var int24AsBytes = new UInt24((uint)chunked[i].Length).Bytes;
                //  mem.Write(int24AsBytes, 0, int24AsBytes.Length);
                progress.Report((double)i / (chunked.Count - 1));
            }

            bw.Close();
            File.WriteAllBytes(Owen.Path.Replace("WindowsClient", FortniteUtil.ConfigData.BackupFileName).Replace(".ucas", ".utoc"), mem.ToArray());
            mem.Close();

            Console.WriteLine();
            WriteLineColored(ConsoleColor.Green, "Done!");
        }
    }
    
    public static void WriteLineColored(ConsoleColor color, string text)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    private static List<byte[]> ChunkData(byte[] ubulkBytes)
    {
        List<byte[]> result = new();
        int remainingLength = ubulkBytes.Length;
        for (int i = 0; remainingLength > 0; i++)
        {
            byte[] chunkData = new byte[remainingLength >= 65536 ? 65536 : remainingLength];
            Array.Copy(ubulkBytes, i * 65536, chunkData, 0, remainingLength >= 65536 ? 65536 : remainingLength);
            result.Add(chunkData);
            remainingLength -= remainingLength >= 65536 ? 65536 : remainingLength;
        }

        return result;
    }
}
