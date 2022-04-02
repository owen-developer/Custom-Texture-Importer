using Custom_Texture_Importer.Models;
using Newtonsoft.Json;

namespace Custom_Texture_Importer.Utils;

public class FortniteUtil
{
    public static Config ConfigData = new() { BackupFileName = "OwenClient" };

    public static string PakPath
        => GetFortnitePath() + @"\FortniteGame\Content\Paks";

    public static async Task CopyFiles(string fileName)
    {
        var fileExts = new[]
        {
            ".pak",
            ".sig",
            ".utoc",
            ".ucas"
        };

        var i = 0;
        foreach (var fileExt in fileExts)
        {
            var path = Path.Combine(PakPath, fileName + fileExt);
            if (!File.Exists(path)) return;

            if (fileExt is ".ucas")
            {
                Parallel.For(0, 20, async (i, state) =>
                {
                    try
                    {
                        var paritionPath = i > 0
                            ? string.Concat(fileName, "_s", i, ".ucas")
                            : string.Concat(fileName, ".ucas");
                        paritionPath = Path.Combine(PakPath, paritionPath);

                        if (!File.Exists(paritionPath))
                        {
                            state.Break();
                            return;
                        }

                        if (File.Exists(paritionPath.Replace("WindowsClient", "SaturnClient"))) return;

                        await using var paritionSource =
                            File.Open(paritionPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        await using var paritionDestination =
                            File.Create(paritionPath.Replace("WindowsClient", "SaturnClient"));
                        await paritionSource.CopyToAsync(paritionDestination);
                    }
                    catch (Exception e)
                    {
                        throw new FileLoadException($"Failed to open container partition {i} for {fileName}", e);
                    }
                });
            }
            else
            {
                var newPath = path.Replace("WindowsClient", "SaturnClient");
                if (File.Exists(newPath)) continue;

                await using var source = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                await using var destination = File.Create(newPath);
                await source.CopyToAsync(destination);
            }
        }
    }

    public static string GetFortnitePath()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Epic\\UnrealEngineLauncher\\LauncherInstalled.dat");

        return !File.Exists(path)
            ? null
            : JsonConvert.DeserializeObject<InstalledApps>(File.ReadAllText(path)).InstallationList
                .FirstOrDefault(x => x.AppName == "Fortnite").InstallLocation;
    }

    public static string GetFortniteVersion()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Epic\\UnrealEngineLauncher\\LauncherInstalled.dat");

        return JsonConvert.DeserializeObject<InstalledApps>(File.ReadAllText(path)).InstallationList
            .FirstOrDefault(x => x.AppName == "Fortnite").AppVersion;
    }
}

public class InstallationList
{
    [JsonProperty("InstallLocation")] public string InstallLocation { get; set; }

    [JsonProperty("NamespaceId")] public string NamespaceId { get; set; }

    [JsonProperty("ItemId")] public string ItemId { get; set; }

    [JsonProperty("ArtifactId")] public string ArtifactId { get; set; }

    [JsonProperty("AppVersion")] public string AppVersion { get; set; }

    [JsonProperty("AppName")] public string AppName { get; set; }
}

public class InstalledApps
{
    [JsonProperty("InstallationList")] public List<InstallationList> InstallationList { get; set; }
}