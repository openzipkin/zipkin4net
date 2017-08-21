using System;

namespace zipkin4net.Transport
{
    public static class ZipkinHttpHeaders
    {
        /// <summary>
        /// See https://twitter.github.io/zipkin/Instrumenting.html#communicating-trace-information
        /// for the description of the HTTP Headers used
        /// </summary>
        public const string TraceId = "X-B3-TraceId";
        public const string SpanId = "X-B3-SpanId";
        public const string ParentSpanId = "X-B3-ParentSpanId";
        public const string Sampled = "X-B3-Sampled"; // Will be replaced by Flags in the future releases of Finagle
        public const string Flags = "X-B3-Flags";

        public static SpanFlags ParseFlagsHeader(string header)
        {
            long flagsLong;

            if (!string.IsNullOrEmpty(header) && long.TryParse(header, out flagsLong))
            {
                return (SpanFlags)flagsLong;
            }

            return SpanFlags.None;
        }

        public static bool? ParseSampledHeader(string header)
        {
            if (header == null) return null;

            if (header.Equals("1", StringComparison.OrdinalIgnoreCase) || header.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (header.Equals("0", StringComparison.OrdinalIgnoreCase) || header.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return null;
        }
    }
}
