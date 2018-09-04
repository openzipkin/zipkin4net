using System;
using System.Text;
using zipkin4net.Utils;

namespace zipkin4net.Propagation
{
    /// 
    /// This format corresponds to the propagation key "b3" (or "B3"), which delimits fields in the
    /// following manner.
    /// 
    /// <pre><code>
    /// b3: {x-b3-traceid}-{x-b3-spanid}-{if x-b3-flags 'd' else x-b3-sampled}-{x-b3-parentspanid}
    /// </code></pre>
    /// 
    /// <p>For example, a sampled root span would look like:
    /// <code>4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-1</code>
    /// 
    /// <p>... a not yet sampled root span would look like:
    /// <code>4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7</code>
    /// 
    /// <p>... and a debug RPC child span would look like:
    /// <code>4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-d-5b4185666d50f68b</code>
    /// 
    /// <p>Like normal B3, it is valid to omit trace identifiers in order to only propagate a sampling
    /// decision. For example, the following are valid downstream hints:
    /// <ul>
    /// <li>don't sample - <code>b3: 0</code></li>
    /// <li>sampled - <code>b3: 1</code></li>
    /// <li>debug - <code>b3: d</code></li>
    /// </ul>
    /// 
    /// Reminder: debug (previously <code>X-B3-Flags: 1</code>), is a boosted sample signal which is recorded
    /// to ensure it reaches the collector tier. See <see cref="ISamplingFlags.Debug"/>.
    /// 
    /// <p>See <a href="https://github.com/openzipkin/b3-propagation">B3 Propagation</a>
    /// 
    public static class B3SingleFormat
    {
        private const int FormatMaxLength = 32 + 1 + 16 + 2 + 16; // traceid128-spanid-1-parentid

        /// <summary>
        /// Writes all B3 defined fields in the trace context, except <see cref="ITraceContext.ParentSpanId"/>,
        /// to a hyphen delimited string.
        /// 
        /// <p>This is appropriate for receivers who understand "b3" single header format, and always do
        /// work in a child span. For example, message consumers always do work in child spans, so message
        /// producers can use this format to save bytes on the wire. On the other hand, RPC clients should
        /// use <see cref="WriteB3SingleFormat(ITraceContext)"/>} instead, as RPC servers often share a span ID
        /// with the client.
        /// </summary>
        /// 
        public static string WriteB3SingleFormatWithoutParentId(ITraceContext context)
        {
            return WriteB3SingleFormat(context, null);
        }

        /// <summary>
        /// Writes all B3 defined fields in the trace context to a hyphen delimited string. This is
        /// appropriate for receivers who understand "b3" single header format.
        /// 
        /// <p>The <see cref="ITraceContext.ParentSpanId"/> is serialized in case the receiver is
        /// an RPC server. When downstream is known to be a messaging consumer, or a server that never
        /// reuses a client's span ID, prefer <see cref="WriteB3SingleFormat(ITraceContext)"/>.
        /// </summary>
        public static string WriteB3SingleFormat(ITraceContext context)
        {
            return WriteB3SingleFormat(context, context.ParentSpanId);
        }
        
        private static string WriteB3SingleFormat(ITraceContext context, long? parentId)
        {
            var result = new StringBuilder(FormatMaxLength);
            
            var traceIdHigh = context.TraceIdHigh;
            if (traceIdHigh != SpanState.NoTraceIdHigh)
            {
                var traceIdHighString = NumberUtils.EncodeLongToLowerHexString(traceIdHigh);
                result.Append(traceIdHighString);
            }

            var traceIdString = NumberUtils.EncodeLongToLowerHexString(context.TraceId);
            result.Append(traceIdString);
            result.Append('-');

            var spanIdString = NumberUtils.EncodeLongToLowerHexString(context.SpanId);
            result.Append(spanIdString);

            var sampled = context.Sampled;
            if (sampled.HasValue || context.Debug)
            {
                result.Append('-');
                result.Append(context.Debug ? 'd' : sampled.Value ? '1' : '0');
            }

            if (parentId.HasValue && parentId.Value != 0L)
            {
                result.Append('-');
                var parentIdString = NumberUtils.EncodeLongToLowerHexString(parentId.Value);
                result.Append(parentIdString);
            }

            return result.ToString();
        }

        public static ITraceContext ParseB3SingleFormat(string b3)
        {
            if (b3 == null)
            {
                return null;
            }
            return ParseB3SingleFormat(b3, 0, b3.Length);
        }

        /// <summary>
        /// <param name="beginIndex">the start index, inclusive</param>
        /// <param name="endIndex">the end index, exclusive</param>
        /// </summary>
        public static ITraceContext ParseB3SingleFormat(string b3, int beginIndex,
            int endIndex)
        {
            if (beginIndex == endIndex)
            {
                return null;
            }

            int pos = beginIndex;
            if (pos + 1 == endIndex)
            {
                // possibly sampling flags
                return TryParseSamplingFlags(b3, pos);
            }

            // At this point we minimally expect a traceId-spanId pair
            if (endIndex < 16 + 1 + 16 /* traceid64-spanid */)
            {
                return null;
            }
            else if (endIndex > FormatMaxLength)
            {
                return null;
            }

            long traceIdHigh, traceId;
            if (b3[pos + 32] == '-')
            {
                traceIdHigh = TryParse16HexCharacters(b3, pos, endIndex);
                pos += 16; // upper 64 bits of the trace ID
                traceId = TryParse16HexCharacters(b3, pos, endIndex);
            }
            else
            {
                traceIdHigh = 0L;
                traceId = TryParse16HexCharacters(b3, pos, endIndex);
            }

            pos += 16; // traceId
            if (!CheckHyphen(b3, pos++)) return null;

            if (traceIdHigh == 0L && traceId == 0L)
            {
                return null;
            }

            var spanId = TryParse16HexCharacters(b3, pos, endIndex);
            if (spanId == 0L)
            {
                return null;
            }

            pos += 16; // spanid

            var flags = SpanFlags.None;
            long? parentId = null;
            if (endIndex > pos)
            {
                // If we are at this point, we have more than just traceId-spanId.
                // If the sampling field is present, we'll have a delimiter 2 characters from now. Ex "-1"
                // If it is absent, but a parent ID is (which is strange), we'll have at least 17 characters.
                // Therefore, if we have less than two characters, the input is truncated.
                if (endIndex == pos + 1)
                {
                    return null;
                }

                if (!CheckHyphen(b3, pos++)) return null;

                // If our position is at the end of the string, or another delimiter is one character past our
                // position, try to read sampled status.
                if (endIndex == pos + 1 || DelimiterFollowsPos(b3, pos, endIndex))
                {
                    flags = ParseFlags(b3, pos);
                    if (flags == 0) return null;
                    pos++; // consume the sampled status
                }

                if (endIndex > pos)
                {
                    // If we are at this point, we should have a parent ID, encoded as "-[0-9a-f]{16}"
                    if (endIndex != pos + 17)
                    {
                        return null;
                    }

                    if (!CheckHyphen(b3, pos++)) return null;
                    parentId = TryParse16HexCharacters(b3, pos, endIndex);
                    if (parentId == 0L)
                    {
                        return null;
                    }
                }
            }

            return new SpanState(traceIdHigh, traceId, parentId, spanId, flags.HasFlag(SpanFlags.Sampled), flags.HasFlag(SpanFlags.Debug));
        }

        static ITraceContext TryParseSamplingFlags(string b3, int pos)
        {
            var flags = ParseFlags(b3, pos);
            if (flags == 0) return null;
            return null; // Not handled yet
        }

        private static bool CheckHyphen(string b3, int pos)
        {
            return b3[pos] == '-';
        }

        private static bool DelimiterFollowsPos(string b3, int pos, int end)
        {
            return (end >= pos + 2) && b3[pos + 1] == '-';
        }

        private static long TryParse16HexCharacters(string lowerHex, int index, int end)
        {
            int endIndex = index + 16;
            if (endIndex > end) return 0L;
            try
            {
                return NumberUtils.DecodeHexString(lowerHex.Substring(index, 16));
            }
            catch (Exception e)
            {
                return 0L;
            }
        }

        private static SpanFlags ParseFlags(string b3, int pos)
        {
            SpanFlags flags;
            var sampledChar = b3[pos];
            switch (sampledChar)
            {
                case 'd':
                    flags = SpanFlags.SamplingKnown | SpanFlags.Sampled | SpanFlags.Debug;
                    break;
                case '1':
                    flags = SpanFlags.SamplingKnown | SpanFlags.Sampled;
                    break;
                case '0':
                    flags = SpanFlags.SamplingKnown;
                    break;
                default:
                    flags = 0;
                    break;
            }

            return flags;
        }
    }
}