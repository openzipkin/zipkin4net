using System.Text.Json.Serialization;

namespace example.message.common
{
    public class Message
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("trace_id")]
        public string TraceId { get; set; }

        [JsonPropertyName("parent_id")]
        public string ParentId { get; set; }

        [JsonPropertyName("span_id")]
        public string SpanId { get; set; }

        [JsonPropertyName("sampled")]
        public string Sampled { get; set; }

        [JsonPropertyName("flags")]
        public long Flags { get; set; }
    }
}
