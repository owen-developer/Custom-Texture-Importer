using Custom_Texture_Importer.Models;
using Custom_Texture_Importer.UI;
using Custom_Texture_Importer.Utils.Program;
using Newtonsoft.Json;

namespace Custom_Texture_Importer.Utils;

public class FortniteUtil
{
    private static string _fortnitePath = GetFortnitePath() + @"\FortniteGame\Content\Paks";
    public static string PakPath
    {
        get
        {
            return _fortnitePath;
        }

        set
        {
            _fortnitePath = value;
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
    
    public static async Task RemoveDupedUcas()
    {
        var files = Directory.GetFiles(PakPath).Where(x => x.Contains(Config.CurrentConfig.BackupFileName)).ToArray();
        await GUI.ProgressBarLoop("Removing duped files...", "Removing duped files", new ForLoop<byte>(files.Length, 0, ctx =>
        {
            var file = files[ctx.Index];
            GUI.Log($"Removing file: {file}");
            File.Delete(file);
        }));
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