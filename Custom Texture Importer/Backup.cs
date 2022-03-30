using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Custom_Texture_Importer
{
    public static class Backup
    {

        // From SaturnSwapper by Tamely
        public static async Task BackupFile(string sourceFile)
        {
            await Task.Run(async () =>
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

                foreach (var (fileExt, path) in from fileExt in fileExts
                                                let path = Path.Combine(FortniteUtil.PakPath, fileName + fileExt)
                                                select (fileExt, path))
                {
                    if (!File.Exists(path))
                    {
                        return;
                    }

                    if (fileExt is ".ucas")
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            try
                            {
                                var paritionPath = i > 0 ? string.Concat(fileName, "_s", i, ".ucas") : string.Concat(fileName, ".ucas");
                                paritionPath = Path.Combine(FortniteUtil.PakPath, paritionPath);

                                if (!File.Exists(paritionPath))
                                    break;

                                if (File.Exists(paritionPath.Replace("WindowsClient", FortniteUtil.ConfigData.BackupFileName)))
                                {
                                    continue;
                                }

                                var bufferLength = 262144;
                                var readBuffer = new Byte[bufferLength];
                                var writeBuffer = new Byte[bufferLength];
                                var readSize = -1;

                                IAsyncResult writeResult;
                                IAsyncResult readResult;

                                await using var sourceStream = new FileStream(paritionPath, FileMode.Open, FileAccess.Read);
                                await using (var destinationStream = new FileStream(paritionPath.Replace("WindowsClient", FortniteUtil.ConfigData.BackupFileName), FileMode.Create, FileAccess.Write, FileShare.None, 8, FileOptions.Asynchronous | FileOptions.SequentialScan))
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
                        var newPath = path.Replace("WindowsClient", FortniteUtil.ConfigData.BackupFileName);
                        if (File.Exists(newPath))
                        {
                            continue;
                        }

                        await using var source = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                        await using var destination = File.Create(newPath);
                        await source.CopyToAsync(destination);
                    }
                }
            });

        }
    }
}
