using CUE4Parse;
using Custom_Texture_Importer.Models;
using Custom_Texture_Importer.Utils;
using Custom_Texture_Importer.Utils.Libs;
using Custom_Texture_Importer.Utils.Program;
using Spectre.Console;
using Custom_Texture_Importer.UI;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CUE4Parse.FileProvider;
using Custom_Texture_Importer.Utils.CommandLineParser;

namespace Custom_Texture_Importer;

public static class Program
{
    public static async Task Main()
    {
        Config.InitConfig();
        FortniteUtil.PakPath = Config.CurrentConfig.PakPath;
        if (Config.CurrentConfig.HelpMessage)
        {
            Evaluator.PrintHelpMessage();
        }

        RichPresenceClient.Start();

        DefaultFileProvider provider = null;
        DefaultFileProvider provider2 = null;
        GUI.ProgressBarAction("Initializing Provider", "Initializing Provider", () =>
        {
            provider = new FileProvider().Provider;
        });

        while (true)
        {
            try
            {
                var (texturePath, isCommand) = RunTask(GUI.Input("Input the path to the texture's ubulk to replace ·"));
                if (isCommand)
                {
                    continue;
                }

                texturePath = texturePath.Replace(".uasset", ".ubulk") ?? throw new Exception("Please input a path");

                RichPresenceClient.UpdatePresence("Made by @owenonhxd", $"Importing Texture {Path.GetFileNameWithoutExtension(texturePath)}");

                Owen.IsExporting = true;
                var originalUbulkBytes = await provider.SaveAssetAsync(texturePath);
                Owen.IsExporting = false;

                GUI.ProgressBarAction("Initializing Secondary Provider", "Initializing Provider", () =>
                {
                    provider2 = new FileProvider().Provider;
                    Owen.IsExporting = true;
                    _ = provider.SaveAsset(texturePath);
                });

                var (customUbulkPath, isCmd) = RunTask(GUI.Input("Input your custom ubulk (must be the same size or smaller) ·"));
                customUbulkPath = customUbulkPath.Replace("\"", string.Empty) ?? throw new FileNotFoundException("A file cannot be \"null\"");
                if (isCmd)
                {
                    continue;
                }

                var customUbulkBytes = await File.ReadAllBytesAsync(customUbulkPath);

                if (customUbulkBytes.Length < originalUbulkBytes.Length)
                {
                    GUI.Log("Fixing small ubulk (EXPERIMENTAL)...");
                    List<byte> result = new(customUbulkBytes);
                    for (int i = 0; i < originalUbulkBytes.Length - customUbulkBytes.Length - 1; i++)
                        result.Add(0);
                    customUbulkBytes = result.ToArray();
                    GUI.Log("Fixed small ubulk.");
                }
                else if (customUbulkBytes.Length > originalUbulkBytes.Length)
                {
                    throw new Exception($"BAD SIZE: Custom UBulk is {customUbulkBytes.Length - originalUbulkBytes.Length} bytes longer than the original");   
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

                var tocOffset = (uint)FileUtil.IndexOfSequence(utocStream.ToArray(), 
                    BitConverter.GetBytes((int)Owen.Offsets.First()));

                var written = (long)0;

                await GUI.ProgressBarLoop("Replacing texture data", "Replacing", new ForLoop<byte[]>(chunkedData.ToArray(), 0, ctx =>
                {
                    var compressedChunk = new Oodle().Compress(chunkedData[ctx.Index]);

                    ucasStream.BaseStream.Position = Owen.Offsets[0] + written;
                    var longAsBytes = BitConverter.GetBytes((int)ucasStream.BaseStream.Position);
                    ucasStream.Write(compressedChunk, 0, compressedChunk.Length);

                    written += compressedChunk.Length + 10;

                    utocStream.Position = Owen.TocOffsets[ctx.Index];
                    utocStream.Write(longAsBytes, 0, longAsBytes.Length);

                    utocStream.Position += 1;
                    var intAsBytesC = BitConverter.GetBytes((ushort)compressedChunk.Length);
                    utocStream.Write(intAsBytesC, 0, intAsBytesC.Length);
                }));

                Owen.Offsets.Clear();
                Owen.TocOffsets.Clear();
                Owen.Paths.Clear();
                
                ucasStream.Close();
                File.WriteAllBytes(Owen.Path.Replace("WindowsClient", Config.CurrentConfig.BackupFileName), utocStream.ToArray());
                utocStream.Close();

                RichPresenceClient.UpdatePresence("Made by @owenonhxd", "Browsing for Texture");
            }
            catch (KeyNotFoundException kne)
            {
                GUI.WriteLineColored(GUI.ERROR_COLOR, "The file you inputted does not exist");
                GUI.WriteLineColored(GUI.ERROR_COLOR, kne.Message);
            }
            catch (FileNotFoundException fnfe)
            {
                GUI.WriteLineColored(GUI.ERROR_COLOR, "The file you inputted does not exist");
                GUI.WriteLineColored(GUI.ERROR_COLOR, fnfe.Message);
            }
            catch (Exception e)
            {
                GUI.WriteLineColored(GUI.ERROR_COLOR, "An error occurred");
                GUI.WriteLineColored(GUI.ERROR_COLOR, e.Message);
            }

            Thread.Sleep(100);
        }
    }

    public static T RunTask<T>(Task<T> task)
    {
        while (!task.IsCompleted)
        {
            continue;
        }

        return task.Result;
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