using System.Text.Json.Serialization;

namespace DaiBot.Plugin.Nanikiru.Model
{
    internal class BaiduTransPayload
    {
        [JsonPropertyName("from")]
        public string? From { get; set; }

        [JsonPropertyName("to")]
        public string? To { get; set; }

        [JsonPropertyName("trans_result")]
        public List<TransResultPayload> TransResult { get; set; } = new();

        [JsonPropertyName("error_code")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("error_msg")]
        public string? ErrorMsg { get; set; }

        public class TransResultPayload
        {
            [JsonPropertyName("src")]
            public string? Sec { get; set; }

            [JsonPropertyName("dst")]
            public string? Dst { get; set; }
        }
    }
}
