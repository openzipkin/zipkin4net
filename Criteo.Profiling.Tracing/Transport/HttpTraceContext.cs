using System;
using System.Collections.Specialized;
using System.Globalization;

namespace Criteo.Profiling.Tracing.Transport
{
    public class HttpTraceContext
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

        public static bool TryGet(NameValueCollection headers, out Trace trace)
        {
            var encodedTraceId = headers[TraceId];
            var encodedSpanId = headers[SpanId];
            var encodedParentSpanId = headers[ParentSpanId];

            if (!String.IsNullOrWhiteSpace(encodedTraceId) && !String.IsNullOrWhiteSpace(encodedSpanId) &&
                !String.IsNullOrWhiteSpace(encodedParentSpanId))
            {
                try
                {
                    var traceId = DecodeHexString(encodedTraceId);
                    var spanId = DecodeHexString(encodedSpanId);
                    var parentSpanId = DecodeHexString(encodedParentSpanId);
                    var sampled = ParseSampledHeader(headers[Sampled]);
                    var flags = ParseFlagsHeader(headers[Flags]);

                    if (sampled != null)
                        flags = sampled.Value ? flags.SetSampled() : flags.SetNotSampled();

                    var id = new SpanId(traceId, parentSpanId, spanId, flags);
                    trace = Trace.CreateFromId(id);
                    return true;
                }
                catch (Exception ex)
                {
                    Trace.Logger.LogWarning("Couldn't parse trace context from HTTP headers. Trace is ignored. Message:" + ex.Message);
                }
            }

            trace = default(Trace);
            return false;
        }

        public static void Set(NameValueCollection headers, Trace trace)
        {
            var traceId = trace.CurrentId;

            headers[TraceId] = EncodeLongToHexString(traceId.TraceId);
            headers[SpanId] = EncodeLongToHexString(traceId.Id);
            headers[ParentSpanId] = EncodeLongToHexString(traceId.ParentSpanId);
            headers[Flags] = traceId.Flags.ToLong().ToString(CultureInfo.InvariantCulture);

            // Add "Sampled" header for compatibility with Finagle
            if (traceId.Flags.IsSamplingKnown())
                headers[Sampled] = traceId.Flags.IsSampled() ? "1" : "0";
        }

        internal static string EncodeLongToHexString(long value)
        {
            return value.ToString("X16");
        }

        internal static long DecodeHexString(String longAsHexString)
        {
            return Convert.ToInt64(longAsHexString, 16);
        }

        private static bool? ParseSampledHeader(String header)
        {
            if (String.Equals(header, "1"))
            {
                return true;
            }

            if (String.Equals(header, "0"))
            {
                return false;
            }

            return null;
        }

        private static Flags ParseFlagsHeader(String header)
        {
            long flagsLong;

            if (!String.IsNullOrEmpty(header) && Int64.TryParse(header, out flagsLong))
            {
                return Tracing.Flags.FromLong(flagsLong);
            }

            return Tracing.Flags.Empty();
        }


    }
}
