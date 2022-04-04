using Newtonsoft.Json;

namespace Custom_Texture_Importer.Models
{
    public static class Config
    {
        private const string ConfigPath = "config.json";

        public static ConfigObj CurrentConfig = new ConfigObj();
        public static void InitConfig()
        {
            if (!File.Exists(ConfigPath))
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(new ConfigObj(), Formatting.Indented));

            CurrentConfig = JsonConvert.DeserializeObject<ConfigObj>(File.ReadAllText(ConfigPath));
        }
        public static void SaveConfig()
        {
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(CurrentConfig));
        }
        public class ConfigObj
        {
            [JsonProperty]
            public string BackupFileName { get; set; } = "OwenClient";
            [JsonProperty]
            public ConsoleColor SystemColor { get; set; } = ConsoleColor.Green;
            [JsonProperty]
            public ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;
            [JsonProperty]
            public ConsoleColor WarningColor { get; set; } = ConsoleColor.Yellow;
            [JsonProperty]
            public ConsoleColor InputColor { get; set; } = ConsoleColor.Cyan;
            [JsonProperty]
            public ConsoleColor ProgressBarColor { get; set; } = ConsoleColor.Blue;
            [JsonProperty]
            public bool RpcIsEnabled { get; set; } = true;
        }
    }
}