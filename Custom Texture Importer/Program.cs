using CUE4Parse;
using Custom_Texture_Importer.Models;
using Custom_Texture_Importer.Utils;
using Custom_Texture_Importer.Utils.Libs;
using Custom_Texture_Importer.Utils.Program;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Custom_Texture_Importer;

public static class Program
{
#pragma warning disable CA2211
    public static ConsoleColor SYSTEM_COLOR = ConsoleColor.Green;
    public static ConsoleColor ERROR_COLOR = ConsoleColor.Red;
    public static ConsoleColor WARNING_COLOR = ConsoleColor.Yellow;
    public static ConsoleColor INPUT_COLOR = ConsoleColor.Cyan;
    public static ConsoleColor PROGRESS_BAR_COLOR = ConsoleColor.Blue;
#pragma warning restore CA2211

    public static async Task Main()
    {
        Config.InitConfig();
        SYSTEM_COLOR = Config.CurrentConfig.SystemColor;
        ERROR_COLOR = Config.CurrentConfig.ErrorColor;
        WARNING_COLOR = Config.CurrentConfig.WarningColor;
        INPUT_COLOR = Config.CurrentConfig.InputColor;
        PROGRESS_BAR_COLOR = Config.CurrentConfig.ProgressBarColor;

        RichPresenceClient.Start();

        var provider = new FileProvider().Provider;

        while (true)
        {
            var texturePath = Input("Input the path to the texture's ubulk to replace OR input 'exit' to close the tool > ", out var isCommand);
            if (isCommand)
                continue;
            
            texturePath = texturePath.Replace(".uasset", ".ubulk");

            if (texturePath is null or "")
            {
                WriteLineColored(ERROR_COLOR, "Please input a path");
                continue;
            }

            RichPresenceClient.UpdatePresence("Made by @owenonhxd", $"Importinng Texture {Path.GetFileNameWithoutExtension(texturePath)}");

            Owen.IsExporting = true;
            await provider.SaveAssetAsync(texturePath);
            var customUbulkPath = Input("Input your custom ubulk (must be the same size) > ", out var isCmd).Replace("\"", string.Empty) ??
                                                          throw new FileNotFoundException("A file cannot be \"null\"");
            if (isCmd)
                continue;
            var ubulkBytes = await File.ReadAllBytesAsync(customUbulkPath);
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
            WriteLineColored(SYSTEM_COLOR, "Processing...");
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

                progress.Report((double)i / (chunked.Count - 1), 50);
            }

            await File.WriteAllBytesAsync(
                Owen.Path.Replace("WindowsClient", Config.CurrentConfig.BackupFileName).Replace(".ucas", ".utoc"),
                utocStream.ToArray());
            
            Console.WriteLine();

            ucasStream.Close();
            utocStream.Close();

            WriteLineColored(SYSTEM_COLOR, "\nDone!");

            RichPresenceClient.UpdatePresence("Made by @owenonhxd", "Browsing for Texture...");
        }
    }

    public static void WriteLineColored(ConsoleColor color, string text)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    public static void WriteColored(ConsoleColor color, string text)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }

    private static string Input(string text, out bool isCommand)
    {
        WriteColored(SYSTEM_COLOR, text);
        Console.ForegroundColor = INPUT_COLOR;
        var input = Console.ReadLine();
        isCommand = CheckForCommands(input).Result;
        Console.ResetColor();
        return input;
    }

    private static async Task<bool> CheckForCommands(string input)
    {
        if (input[0] == '#')
        {
            input = input[1..].ToLower();
            switch (input)
            {
                case "exit":
                    Environment.Exit(0);
                    break;
                case "cls":
                    Console.Clear();
                    break;
                case "restore":
                    FortniteUtil.RemoveDupedUcas();
                    WriteLineColored(SYSTEM_COLOR, "Removed duped files!");
                    break;
                case "backup":
                    await Backup.BackupFile(Owen.Path);
                    WriteLineColored(SYSTEM_COLOR, "Backed up files");
                    break;
                case "colors":
                    // Print each color and it's value from ConsoleColor enum
                    foreach (var color in Enum.GetValues(typeof(ConsoleColor)))
                    {
                        WriteLineColored((ConsoleColor)color, $"{color} = {(int)color}");
                    }
                    break;
                case "help":
                    WriteLineColored(SYSTEM_COLOR, "Commands:");
                    WriteLineColored(SYSTEM_COLOR, "exit - Exits the program");
                    WriteLineColored(SYSTEM_COLOR, "cls - Clears the console");
                    WriteLineColored(SYSTEM_COLOR, "restore - Removes duped files");
                    WriteLineColored(SYSTEM_COLOR, "backup - Backups the current file");
                    WriteLineColored(SYSTEM_COLOR, "colors - Prints all the available colors you can use for UI colors.");
                    WriteLineColored(SYSTEM_COLOR, "help - Shows this message");
                    break;
                default:
                    WriteLineColored(ERROR_COLOR, "Invalid command");
                    return false;
            }

            return true;
        }

        return false;
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