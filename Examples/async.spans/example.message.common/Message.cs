using System.Runtime.Serialization;

namespace example.message.common
{
    [DataContract(Name = "message")]
    public class Message
    {
        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "trace_id")]
        public string TraceId { get; set; }

        [DataMember(Name = "parent_id")]
        public string ParentId { get; set; }

        [DataMember(Name = "span_id")]
        public string SpanId { get; set; }

        [DataMember(Name = "sampled")]
        public string Sampled { get; set; }

        [DataMember(Name = "flags")]
        public long Flags { get; set; }
    }
}
