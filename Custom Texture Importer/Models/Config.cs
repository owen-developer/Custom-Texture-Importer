using Newtonsoft.Json;

namespace Custom_Texture_Importer.Models
{
    public class Config
    {
        private const string ConfigPath = "config.json";

        public static ConfigObj CurrentConfig = new ConfigObj();
        public static void InitConfig()
        {
            if (!File.Exists(ConfigPath))
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(new ConfigObj()));

            CurrentConfig = JsonConvert.DeserializeObject<ConfigObj>(File.ReadAllText(ConfigPath));
        }
        public static void SaveConfig()
        {
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(CurrentConfig));
        }
        public class ConfigObj
        {
            public string BackupFileName { get; set; } = "OwenClient";
            public bool rpcIsEnabled { get; set; } = true;
        }
    }
}