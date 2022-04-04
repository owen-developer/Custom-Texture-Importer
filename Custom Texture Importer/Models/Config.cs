using Newtonsoft.Json;
using System.Reflection;

namespace Custom_Texture_Importer.Models
{
    public static class Config
    {
        public const string CONFIG_PATH = "config.json";

        public static ConfigObj CurrentConfig = new ConfigObj();
        public static void InitConfig()
        {
            if (!File.Exists(CONFIG_PATH))
                File.WriteAllText(CONFIG_PATH, JsonConvert.SerializeObject(new ConfigObj(), Formatting.Indented));

            CurrentConfig = JsonConvert.DeserializeObject<ConfigObj>(File.ReadAllText(CONFIG_PATH));
            SaveConfig();
        }
        public static void SaveConfig()
        {
            File.WriteAllText(CONFIG_PATH, JsonConvert.SerializeObject(CurrentConfig));
        }
        public class ConfigObj
        {
            [JsonProperty]
            public string BackupFileName { get; set; } = "OwenClient";
            [JsonProperty]
            public ConsoleColor InfoColor { get; set; } = ConsoleColor.Green;
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

            /// <summary>
            /// This ToString returns a string representation of each JsonProperty in the current instance of this class.
            /// This way we never need to add a new string to the ToString, instead it gets all properties name and value.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                var properties = typeof(ConfigObj).GetProperties();
                var serializableProperties = new List<PropertyInfo>();
                foreach (var property in properties)
                {
                    if (property.GetCustomAttribute<JsonPropertyAttribute>() != null)
                    {
                        serializableProperties.Add(property);
                    }
                }

                var result = "";
                foreach (var property in serializableProperties)
                {
                    result += $"{property.Name}: {property.GetValue(this)}\n";
                }

                return result;
            }
        }
    }
}