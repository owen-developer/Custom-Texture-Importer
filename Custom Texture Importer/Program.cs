using CUE4Parse;
using Custom_Texture_Importer.Models;
using Custom_Texture_Importer.Utils;
using Custom_Texture_Importer.Utils.Libs;
using Custom_Texture_Importer.Utils.Program;
using DiscordRPC;
using Newtonsoft.Json;

namespace Custom_Texture_Importer;

public static class Program
{
    public static async Task Main()
    {
        Config.InitConfig();
        
        RichPresenceClient.Start();

        var provider = new MyFileProvider().Provider;

        while (true)
        {
            Console.Write("Input the path to the texture's ubulk to replace OR input 'exit' to close the tool > ");
            var texturePath = Console.ReadLine()?.Replace(".uasset", ".ubulk");

            if (texturePath == "exit")
                break;

            if (texturePath is null or "")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                WriteLineColored(ConsoleColor.Red, "Please input a path");
                Console.ResetColor();
                continue;
            }

            RichPresenceClient.UpdatePresence("Made by @owenonhxd", $"Importinng Texture {Path.GetFileNameWithoutExtension(texturePath)}");

            Owen.IsExporting = true;
            await provider.SaveAssetAsync(texturePath);
            Console.Write("Input your custom ubulk (must be the same size) > ");
            var ubulkBytes = await File.ReadAllBytesAsync(Console.ReadLine()?.Replace("\"", string.Empty) ??
                                                          throw new FileNotFoundException("A file cannot be \"null\""));
            var chunked = ChunkData(ubulkBytes);

            await Backup.BackupFile(Owen.Path);

            var ucasStream = new BinaryWriter(File.OpenWrite(Owen.Partition == 0
                ? Owen.Path.Replace("WindowsClient", Config.CurrentConfig.BackupFileName).Replace(".utoc", ".ucas")
                : Owen.Path.Replace("WindowsClient", Config.CurrentConfig.BackupFileName + "_s" + Owen.Partition)
                    .Replace(".utoc", ".ucas")));
            var utocStream = new MemoryStream(
                File.Exists(Owen.Path.Replace("WindowsClient", Config.CurrentConfig.BackupFileName))
                    ? await File.ReadAllBytesAsync(Owen.Path.Replace("WindowsClient",
                        Config.CurrentConfig.BackupFileName))
                    : await File.ReadAllBytesAsync(Owen.Path));
            var tocOffset =
                (uint)FileUtil.IndexOfSequence(utocStream.ToArray(), BitConverter.GetBytes((int)Owen.FirstOffset));
            uint written = 0;

            Console.WriteLine();
            WriteLineColored(ConsoleColor.Green, "Processing...");
            var progress = new ProgressBar();
            for (var i = 0; i < chunked.Count; i++)
            {
                var compressed = Oodle.Compress(chunked[i]);

                ucasStream.BaseStream.Position = Owen.FirstOffset + written;
                ucasStream.Write(compressed, 0, compressed.Length);
                written += (uint)compressed.Length + 10;

                utocStream.Position = tocOffset + i * 12;
                var longAsBytes = BitConverter.GetBytes((int)ucasStream.BaseStream.Position);
                utocStream.Write(longAsBytes, 0, longAsBytes.Length);
                utocStream.Position += 1;

                var intAsBytes = BitConverter.GetBytes((ushort)compressed.Length);
                utocStream.Write(intAsBytes, 0, intAsBytes.Length);

                progress.Report((double)i / (chunked.Count - 1));
            }

            await File.WriteAllBytesAsync(
                Owen.Path.Replace("WindowsClient", Config.CurrentConfig.BackupFileName).Replace(".ucas", ".utoc"),
                utocStream.ToArray());

            ucasStream.Close();
            utocStream.Close();

            WriteLineColored(ConsoleColor.Green, "\nDone!");

            RichPresenceClient.UpdatePresence("Made by @owenonhxd", "Browsing for Texture...");
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
        var remainingLength = ubulkBytes.Length;
        for (var i = 0; remainingLength > 0; i++)
        {
            var chunkData = new byte[remainingLength >= 65536 ? 65536 : remainingLength];
            Array.Copy(ubulkBytes, i * 65536, chunkData, 0, remainingLength >= 65536 ? 65536 : remainingLength);
            result.Add(chunkData);
            remainingLength -= remainingLength >= 65536 ? 65536 : remainingLength;
        }

        return result;
    }
}