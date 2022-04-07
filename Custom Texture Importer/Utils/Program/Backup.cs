using Custom_Texture_Importer.UI;

namespace Custom_Texture_Importer.Utils.Program;

public static class Backup
{
    // From SaturnSwapper by Tamely
    public static async Task BackupFile(string sourceFile)
    {
        var fileName = Path.GetFileNameWithoutExtension(sourceFile);
        var fileExts = new[]
        {
            ".pak",
            ".sig",
            ".utoc",
            ".ucas"
        };

        if (fileName.Contains("ient_s")) // Check if it's partitioned
            fileName = fileName.Split("ient_s")[0] + "ient"; // Remove the partition from the name because they don't get utocs

        await GUI.ProgressBarLoop("Backing up files", "Backing up", new ForLoop<string>(fileExts, 0, async ctx =>
        {
            var fileExt = fileExts[ctx.Index];
            var path = Path.Combine(FortniteUtil.PakPath, fileName + fileExt);
            if (!File.Exists(path))
            {
                ctx.Break = true;
                return;
            }

            if (fileExt == ".ucas")
            {
                for (var i = 0; i < 20; i++)
                {
                    try
                    {
                        var paritionPath = i > 0
                            ? string.Concat(fileName, "_s", i, ".ucas")
                            : string.Concat(fileName, ".ucas");
                        paritionPath = Path.Combine(FortniteUtil.PakPath, paritionPath);

                        if (!File.Exists(paritionPath))
                        {
                            break;
                        }

                        GUI.Log($"Backing up partition: {paritionPath}");
                        if (File.Exists(paritionPath.Replace("WindowsClient", Models.Config.CurrentConfig.BackupFileName)))
                        {
                            continue;
                        }

                        var bufferLength = 262144;
                        var readBuffer = new byte[bufferLength];
                        var writeBuffer = new byte[bufferLength];
                        var readSize = -1;

                        IAsyncResult writeResult;
                        IAsyncResult readResult;

                        await using var sourceStream = new FileStream(paritionPath, FileMode.Open, FileAccess.Read);
                        await using (var destinationStream = new FileStream(
                                         paritionPath.Replace("WindowsClient", Models.Config.CurrentConfig.BackupFileName),
                                         FileMode.Create, FileAccess.Write, FileShare.None, 8,
                                         FileOptions.Asynchronous | FileOptions.SequentialScan))
                        {
                            destinationStream.SetLength(sourceStream.Length);
                            readSize = sourceStream.Read(readBuffer, 0, readBuffer.Length);
                            readBuffer = Interlocked.Exchange(ref writeBuffer, readBuffer);

                            while (readSize > 0)
                            {
                                writeResult = destinationStream.BeginWrite(writeBuffer, 0, readSize, null, null);
                                readResult = sourceStream.BeginRead(readBuffer, 0, readBuffer.Length, null, null);
                                destinationStream.EndWrite(writeResult);
                                readSize = sourceStream.EndRead(readResult);
                                readBuffer = Interlocked.Exchange(ref writeBuffer, readBuffer);
                            }

                            sourceStream.Close();
                            destinationStream.Close();
                        }
                    }
                    catch (Exception e)
                    {
                        throw new FileLoadException($"Failed to open container partition {i} for {fileName}", e);
                    }
                }
            }
            else
            {
                var newPath = path.Replace("WindowsClient", Models.Config.CurrentConfig.BackupFileName);
                GUI.Log($"Backing up file: {path}");
                if (File.Exists(newPath))
                {
                    return;
                }

                await using var source = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                await using var destination = File.Create(newPath);
                await source.CopyToAsync(destination);
            }
        }));
    }
}