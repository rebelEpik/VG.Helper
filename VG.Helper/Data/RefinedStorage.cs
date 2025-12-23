using System.Text.Json.Serialization;

namespace VG.Helper.Data
{
    public class RefinedStorage
    {
        [JsonPropertyName("0")]
        public string titanium { get; set; }
        [JsonPropertyName("1")]
        public string oxide { get; set; }
        [JsonPropertyName("2")]
        public string silicon { get; set; }
        [JsonPropertyName("3")]
        public string tungsten { get; set; }
        [JsonPropertyName("4")]
        public string carbon { get; set; }
        [JsonPropertyName("5")]
        public string iridium { get; set; }
        [JsonPropertyName("6")]
        public string platinum { get; set; }
        [JsonPropertyName("7")]
        public string astatine { get; set; }

    }
}
