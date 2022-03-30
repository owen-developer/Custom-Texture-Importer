using Newtonsoft.Json;

namespace Custom_Texture_Importer
{
    public class DynamicKey
    {
        [JsonProperty("pakFilename")] public string PakFilename { get; set; }

        [JsonProperty("pakGuid")] public string PakGuid { get; set; }

        [JsonProperty("key")] public string Key { get; set; }
    }

    public class Data
    {
        [JsonProperty("build")] public string Build { get; set; }

        [JsonProperty("mainKey")] public string MainKey { get; set; }

        [JsonProperty("dynamicKeys")] public List<DynamicKey> DynamicKeys { get; set; }

        [JsonProperty("updated")] public DateTime Updated { get; set; }
    }

    public class AES
    {
        [JsonProperty("status")] public int Status { get; set; }

        [JsonProperty("data")] public Data Data { get; set; }
    }
}
