using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Utils;

namespace zipkin4net.Benchmark.Tracers.Zipkin.jsondotnet
{
    public class Span
    {
        private readonly zipkin4net.Tracers.Zipkin.Span span;

        public Span(zipkin4net.Tracers.Zipkin.Span span)
        {
            this.span = span;
        }

        [JsonProperty("traceId")]
        public long TraceId => span.SpanState.TraceId;
        [JsonProperty("id")]
        public long Id => span.SpanState.SpanId;
        [JsonProperty("name")]
        public string Name => span.Name;
        [JsonProperty("parentId", NullValueHandling = NullValueHandling.Ignore)]
        public long? ParentId => span.SpanState.ParentSpanId;
        [JsonProperty("annotations")]
        public IEnumerable<Annotation> Annotations => span.Annotations.Select(annotation => new Annotation(annotation, span.Endpoint, span.ServiceName));
        [JsonProperty("binaryAnnotations")]
        public IEnumerable<BinaryAnnotation> BinaryAnnotations => span.BinaryAnnotations.Select(annotation => new BinaryAnnotation(annotation, span.Endpoint, span.ServiceName));
    }

    public class Annotation
    {
        private readonly ZipkinAnnotation annotation;
        private readonly IPEndPoint endpoint;
        private readonly string serviceName;

        public Annotation(ZipkinAnnotation annotation, IPEndPoint endpoint, string serviceName)
        {
            this.annotation = annotation;
            this.endpoint = endpoint;
            this.serviceName = serviceName;
        }

        [JsonProperty("value")]
        public string Value => annotation.Value;
        [JsonProperty("timestamp")]
        public long Timestamp => annotation.Timestamp.ToUnixTimestamp();
        [JsonProperty("endpoint")]
        public Endpoint Endpoint => new Endpoint(endpoint, serviceName);

    }

    public class BinaryAnnotation
    {
        private zipkin4net.Tracers.Zipkin.BinaryAnnotation binaryAnnotation;
        private IPEndPoint endpoint;
        private readonly string serviceName;

        public BinaryAnnotation(zipkin4net.Tracers.Zipkin.BinaryAnnotation binaryAnnotation, IPEndPoint endpoint, string serviceName)
        {
            this.binaryAnnotation = binaryAnnotation;
            this.endpoint = endpoint;
            this.serviceName = serviceName;
        }

        [JsonProperty("key")]
        public string Key => binaryAnnotation.Key;
        [JsonProperty("value")]
        public string Value => Encoding.UTF8.GetString(binaryAnnotation.Value);
        [JsonProperty("endpoint")]
        public Endpoint Endpoint => new Endpoint(endpoint, serviceName);
    }

    public class Endpoint
    {
        private readonly IPEndPoint endpoint;
        private readonly string serviceName;

        public Endpoint(IPEndPoint endpoint, string serviceName)
        {
            this.endpoint = endpoint;
            this.serviceName = serviceName;
        }

        [JsonProperty("ipv4")]
        public int IPv4 => SerializerUtils.IpToInt(endpoint.Address);
        [JsonProperty("port")]
        public int Port => endpoint.Port;
        [JsonProperty("serviceName")]
        public string ServiceName => serviceName;
    }
}