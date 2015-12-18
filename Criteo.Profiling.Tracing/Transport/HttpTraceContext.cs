using System;
using System.Collections.Generic;
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

        public static bool TryGet(IDictionary<string, string> headers, out Trace trace)
        {
            string encodedTraceId, encodedSpanId;

            if (headers.TryGetValue(TraceId, out encodedTraceId)
                && headers.TryGetValue(SpanId, out encodedSpanId))
            {
                string flagsStr, sampledStr, encodedParentSpanId;
                headers.TryGetValue(Flags, out flagsStr);
                headers.TryGetValue(Sampled, out sampledStr);
                headers.TryGetValue(ParentSpanId, out encodedParentSpanId);
                return TryParseTrace(encodedTraceId, encodedSpanId, encodedParentSpanId, sampledStr, flagsStr, out trace);
            }

            trace = default(Trace);
            return false;
        }

        public static bool TryGet(NameValueCollection headers, out Trace trace)
        {
            return TryParseTrace(headers[TraceId], headers[SpanId], headers[ParentSpanId], headers[Sampled], headers[Flags], out trace);
        }

        internal static bool TryParseTrace(string encodedTraceId, string encodedSpanId, string encodedParentSpanId, string sampledStr, string flagsStr, out Trace trace)
        {
            if (String.IsNullOrWhiteSpace(encodedTraceId)
                || String.IsNullOrWhiteSpace(encodedSpanId))
            {
                trace = default(Trace);
                return false;
            }

            try
            {
                var traceId = DecodeHexString(encodedTraceId);
                var spanId = DecodeHexString(encodedSpanId);
                var parentSpanId = String.IsNullOrWhiteSpace(encodedParentSpanId) ? null : (long?)DecodeHexString(encodedParentSpanId);
                var flags = ParseFlagsHeader(flagsStr);
                var sampled = ParseSampledHeader(sampledStr);

                if (sampled != null)
                {
                    flags = Tracing.Flags.Empty(); // "sampled" header overrides all flags
                    flags = sampled.Value ? flags.SetSampled() : flags.SetNotSampled();
                }


                var id = new SpanId(traceId, parentSpanId, spanId, flags);
                trace = Trace.CreateFromId(id);
                return true;
            }
            catch (Exception ex)
            {
                Trace.Logger.LogWarning("Couldn't parse trace context. Trace is ignored. Message:" + ex.Message);
            }

            trace = default(Trace);
            return false;
        }

        // Duplicate code... Don't know any way to avoid this
        public static void Set(NameValueCollection headers, Trace trace)
        {
            var traceId = trace.CurrentId;

            headers[TraceId] = EncodeLongToHexString(traceId.TraceId);
            headers[SpanId] = EncodeLongToHexString(traceId.Id);
            if (traceId.ParentSpanId != null)
            {
                // Cannot be null in theory, the root span must have been created on request receive hence further RPC calls are necessary children
                headers[ParentSpanId] = EncodeLongToHexString(traceId.ParentSpanId.Value);
            }
            headers[Flags] = traceId.Flags.ToLong().ToString(CultureInfo.InvariantCulture);

            // Add "Sampled" header for compatibility with Finagle
            if (traceId.Flags.IsSamplingKnown())
            {
                headers[Sampled] = traceId.Flags.IsSampled() ? "1" : "0";
            }
        }

        // Duplicate code... Don't know any way to avoid this
        public static void Set(IDictionary<string, string> headers, Trace trace)
        {
            var traceId = trace.CurrentId;

            headers[TraceId] = EncodeLongToHexString(traceId.TraceId);
            headers[SpanId] = EncodeLongToHexString(traceId.Id);
            if (traceId.ParentSpanId != null)
            {
                // Cannot be null in theory, the root span must have been created on request receive hence further RPC calls are necessary children
                headers[ParentSpanId] = EncodeLongToHexString(traceId.ParentSpanId.Value);
            }
            headers[Flags] = traceId.Flags.ToLong().ToString(CultureInfo.InvariantCulture);

            // Add "Sampled" header for compatibility with Finagle
            if (traceId.Flags.IsSamplingKnown())
            {
                headers[Sampled] = traceId.Flags.IsSampled() ? "1" : "0";
            }
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
