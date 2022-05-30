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
using Custom_Texture_Importer.CustomUI;
using CUE4Parse.Utils;

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
                var isUi = !provider.TrySaveAsset(texturePath, out var originalUbulkBytes);
                if (isUi) provider.TrySaveAsset(texturePath.Replace(".ubulk", ".uasset"), out originalUbulkBytes);
                Owen.IsExporting = false;

                GUI.ProgressBarAction("Initializing Secondary Provider", "Initializing Provider", () =>
                {
                    provider2 = new FileProvider().Provider;
                    Owen.IsExporting = true;
                    _ = provider.TrySaveAsset(texturePath, out _);
                    Owen.IsExporting = false;
                });

                var (customUbulkPath, isCmd) = RunTask(GUI.Input("Input your custom ubulk (can be ingored if texture is in group: UI) ·"));
                customUbulkPath = customUbulkPath.Replace("\"", string.Empty);
                if (isCmd)
                {
                    continue;
                }

                var customUbulkBytes = File.Exists(customUbulkPath) ? await File.ReadAllBytesAsync(customUbulkPath) : null;

                await Backup.BackupFile(Owen.Path);

                if (isUi)
                {
                    var (customPaksDir, _) = RunTask(GUI.Input("Input your custom ucas/utoc ·"));
                    var uiImporter = new UITextureSupport(customPaksDir.Replace("\"", string.Empty).SubstringBeforeLast('\\'));
                    customUbulkBytes = uiImporter.PrepImport(texturePath.Replace(".ubulk", ".uasset"));
                }
                
                if (customUbulkBytes.Length > originalUbulkBytes.Length)
                {
                    throw new Exception($"BAD SIZE: Custom UBulk is {customUbulkBytes.Length - originalUbulkBytes.Length} bytes longer than the original");
                }

                var chunkedData = ChunkData(customUbulkBytes);

                var ucasStream = new BinaryWriter(File.OpenWrite(Owen.Partition == 0
                ? Owen.Path.Replace("WindowsClient", Config.CurrentConfig.BackupFileName).Replace(".utoc", ".ucas")
                : Owen.Path.Replace("WindowsClient", Config.CurrentConfig.BackupFileName + "_s" + Owen.Partition)
                           .Replace(".utoc", ".ucas")));

                var utocStream = new MemoryStream(
                    File.Exists(Owen.Path.Replace("WindowsClient", Config.CurrentConfig.BackupFileName))
                        ? await File.ReadAllBytesAsync(Owen.Path.Replace("WindowsClient",
                            Config.CurrentConfig.BackupFileName))
                        : await File.ReadAllBytesAsync(Owen.Path));

                var written = 0L;

                await GUI.ProgressBarLoop("Replacing texture data", "Replacing", new ForLoop<byte[]>(chunkedData.ToArray(), 0, ctx =>
                {
                    var compressedChunk = new Oodle().Compress(chunkedData[ctx.Index]);

                    ucasStream.BaseStream.Position = Owen.Offsets[0] + written;
                    var longAsBytes = BitConverter.GetBytes((uint)ucasStream.BaseStream.Position);
                    ucasStream.Write(compressedChunk, 0, compressedChunk.Length);

                    written += compressedChunk.Length + 10;

                    utocStream.Position = Owen.Reader.TocResource.CompressionBlocks[Owen.FirstBlockIndex + ctx.Index].Position;
                    utocStream.Write(longAsBytes, 0, longAsBytes.Length);

                    utocStream.Position += 1;
                    var intAsBytesC = BitConverter.GetBytes((ushort)compressedChunk.Length);
                    utocStream.Write(intAsBytesC, 0, intAsBytesC.Length);

                    utocStream.Position += 1;
                    var intAsBytesD = BitConverter.GetBytes((ushort)chunkedData[ctx.Index].Length);
                    utocStream.Write(intAsBytesD, 0, intAsBytesD.Length);

                    utocStream.Position += 1;
                    var bIsCompressed = BitConverter.GetBytes((byte)1);
                    utocStream.Write(bIsCompressed, 0, bIsCompressed.Length);
                }));

                utocStream.Position = Owen.Reader.TocResource.ChunkOffsetLengths[Owen.TocIndex].Position + 6;
                utocStream.Write(BitConverter.GetBytes(customUbulkBytes.Length).Reverse().ToArray(), 0, 4);

                Owen.Offsets.Clear();
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