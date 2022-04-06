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
#pragma warning disable CA2211, IDE0090
    public static ConsoleColor INFO_COLOR = ConsoleColor.Green;
    public static ConsoleColor ERROR_COLOR = ConsoleColor.Red;
    public static ConsoleColor WARNING_COLOR = ConsoleColor.Yellow;
    public static ConsoleColor INPUT_COLOR = ConsoleColor.Cyan;
    public static ConsoleColor PROGRESS_BAR_COLOR = ConsoleColor.Blue;
    private static readonly object _lock = new object();
#pragma warning restore CA2211, IDE0090

    public static async Task Main()
    {
        Config.InitConfig();
        INFO_COLOR = Config.CurrentConfig.InfoColor;
        ERROR_COLOR = Config.CurrentConfig.ErrorColor;
        WARNING_COLOR = Config.CurrentConfig.WarningColor;
        INPUT_COLOR = Config.CurrentConfig.InputColor;
        PROGRESS_BAR_COLOR = Config.CurrentConfig.ProgressBarColor;
        FortniteUtil.PakPath = Config.CurrentConfig.PakPath;

        RichPresenceClient.Start();

        WriteLineColored(INFO_COLOR, "Initializing Provider...");
        var pb = new ProgressBar();
        var provider = new FileProvider().Provider;
        pb.Report(1, 500);
        WriteLineColored(INFO_COLOR, "Provider Initialized!");
        pb.Dispose();

        while (true)
        {
            try
            {
                var texturePath = Input("Input the path to the texture's ubulk to replace > ", out var isCommand);
                if (isCommand)
                {
                    continue;
                }

                texturePath = texturePath.Replace(".uasset", ".ubulk");

                if (texturePath is null or "")
                {
                    WriteLineColored(ERROR_COLOR, "Please input a path");
                    continue;
                }

                RichPresenceClient.UpdatePresence("Made by @owenonhxd", $"Importinng Texture {Path.GetFileNameWithoutExtension(texturePath)}");

                Owen.IsExporting = true;
                var originalUbulkBytes = await provider.SaveAssetAsync(texturePath);
                var customUbulkPath = Input("Input your custom ubulk (must be the same size) > ", out var isCmd).Replace("\"", string.Empty) ??
                                                              throw new FileNotFoundException("A file cannot be \"null\"");
                if (isCmd)
                {
                    continue;
                }
                
                var customUbulkBytes = await File.ReadAllBytesAsync(customUbulkPath);
                if (customUbulkBytes.Length != originalUbulkBytes.Length)
                {
                    WriteLineColored(ERROR_COLOR, "The ubulk you provided is not the same size as the original");
                    continue;
                }

                var chunkedData = ChunkData(customUbulkBytes);
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

                var tocOffset = FileUtil.IndexOfSequence(utocStream.ToArray(), 
                                                         BitConverter.GetBytes((int)Owen.Offsets.ElementAt(0)));
                var written = (long)0;

                WriteLineColored(INFO_COLOR, "Replacing texture data...");
                var progressBar = new ProgressBar();
                for (int i = 0; i < chunkedData.Count; i++)
                {
                    var compressedChunk = new Oodle().Compress(chunkedData[i]);
                    ucasStream.BaseStream.Position = Owen.Offsets.Pop();
                    var longAsBytes = BitConverter.GetBytes((int)ucasStream.BaseStream.Position);
                    ucasStream.Write(compressedChunk, 0, compressedChunk.Length);
                    written += compressedChunk.Length + 10;
                    utocStream.Position = tocOffset + (i * 12);
                    utocStream.Write(longAsBytes, 0, longAsBytes.Length);
                    utocStream.Position += 1;
                    var intAsBytes = BitConverter.GetBytes((ushort)compressedChunk.Length);
                    utocStream.Write(intAsBytes, 0, intAsBytes.Length);

                    progressBar.Report((double)i / (chunkedData.Count - 1), 500);
                }

                WriteLineColored(INFO_COLOR, "\nDone!");

                RichPresenceClient.UpdatePresence("Made by @owenonhxd", "Browsing for Texture...");
            }
            catch (KeyNotFoundException kne)
            {
                WriteLineColored(ERROR_COLOR, "The file you inputted does not exist");
                WriteLineColored(ERROR_COLOR, kne.Message);
            }
            catch (FileNotFoundException fnfe)
            {
                WriteLineColored(ERROR_COLOR, "The file you inputted does not exist");
                WriteLineColored(ERROR_COLOR, fnfe.Message);
            }
            catch (Exception e)
            {
                WriteLineColored(ERROR_COLOR, "An error occurred");
                WriteLineColored(ERROR_COLOR, e.Message);
            }

            Thread.Sleep(100);
        }
    }

    public static void WriteLineColored(ConsoleColor color, string text)
    {
        lock (_lock)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
            Thread.Sleep(50);
        }
    }

    public static void WriteColored(ConsoleColor color, string text)
    {
        lock (_lock)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
            Thread.Sleep(50);
        }
    }

    private static string Input(string text, out bool isCommand)
    {
        string input = null;
        lock (_lock)
        {
            WriteColored(INFO_COLOR, text);
            Console.ForegroundColor = INPUT_COLOR;
            input = Console.ReadLine();
            isCommand = CheckForCommands(input);
            Thread.Sleep(50);
            Console.ResetColor();
            Thread.Sleep(50);
        }
        
        return input;
    }

    private static bool CheckForCommands(string input)
    {
        unsafe
        {
            string s = "Hello";
            fixed (char* c = s)
            {
                *c = 'H';
            }
        }
        
        if (input[0] == '#')
        {
            input = input[1..(!input.Contains(' ') ? input.Length : input.IndexOf(' '))].ToLower();
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
                    WriteLineColored(INFO_COLOR, "Removed duped files!");
                    break;
                case "colors":
                    // Print each color and it's value from ConsoleColor enum
                    foreach (var color in Enum.GetValues(typeof(ConsoleColor)))
                    {
                        WriteLineColored((ConsoleColor)color, $"{color} = {(int)color}");
                    }
                    break;
                case "config":
                    WriteLineColored(INFO_COLOR, "Current Config:");
                    Console.WriteLine();
                    WriteLineColored(INFO_COLOR, Config.CurrentConfig.ToString());
                    break;
                case "config.open":
                    Process.Start("Notepad", Config.CONFIG_PATH);
                    break;
                case "pakpath.edit":
                    var path = Console.ReadLine();
                    Config.CurrentConfig.PakPath = path;
                    Config.SaveConfig();
                    break;
                case "help":
                    WriteLineColored(INFO_COLOR, "Commands:");
                    WriteLineColored(INFO_COLOR, "exit - Exits the program");
                    WriteLineColored(INFO_COLOR, "cls - Clears the console");
                    WriteLineColored(INFO_COLOR, "restore - Removes duped files");
                    WriteLineColored(INFO_COLOR, "backup - Backups the current file");
                    WriteLineColored(INFO_COLOR, "colors - Prints all the available colors you can use for UI colors.");
                    WriteLineColored(INFO_COLOR, "config - Prints the current config");
                    WriteLineColored(INFO_COLOR, "config.open - Opens the config file in notepad");
                    WriteLineColored(INFO_COLOR, "help - Shows this message");
                    break;
                default:
                    WriteLineColored(ERROR_COLOR, $"Unknown command '{input}'.");
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