using System;
using System.Collections.Generic;
using System.Linq;

namespace zipkin4net.Internal.V2
{
    /// <summary>
    /// A trace is a series of spans (often RPC calls) which form a latency tree.
    /// 
    /// <p>Spans are usually created by instrumentation in RPC clients or servers, but can also represent
    /// in-process activity. Annotations in spans are similar to log statements, and are sometimes
    /// created directly by application developers to indicate events of interest, such as a cache miss.
    /// 
    /// <p>The root span is where <see cref="ParentId"/> is null; it usually has the longest <see cref="Duration"/>
    /// in the trace.
    /// 
    /// <p>Span identifiers are packed into longs, but should be treated opaquely. ID encoding is 16 or
    /// 32 character lower-hex, to avoid signed interpretation.
    /// </summary>
    internal struct Span
    {
        private const int FLAG_DEBUG = 1 << 1;
        private const int FLAG_DEBUG_SET = 1 << 2;
        private const int FLAG_SHARED = 1 << 3;
        private const int FLAG_SHARED_SET = 1 << 4;

        private readonly int flags; // bit field for timestamp and duration, saving 2 object references

        public static Builder NewBuilder()
        {
            return new Builder();
        }

        public Builder ToBuilder()
        {
            return new Builder(this);
        }

        /// <summary>
        /// Trace identifier, set on all spans within it.
        ///
        /// <p>Encoded as 16 or 32 lowercase hex characters corresponding to 64 or 128 bits. For example, a
        /// 128bit trace ID looks like <code>4e441824ec2b6a44ffdc9bb9a6453df3</code>.
        ///
        /// <p>Some systems downgrade trace identifiers to 64bit by dropping the left-most 16 characters.
        /// For example, <code>4e441824ec2b6a44ffdc9bb9a6453df3</code> becomes <code>ffdc9bb9a6453df3</code>.
        /// </summary>
        public readonly string TraceId;

        /// <summary>
        /// The parent's <see cref="Id"/> or null if this the root span in a trace.
        /// 
        /// <p>This is the same encoding as <see cref="Id"/>. For example <code>ffdc9bb9a6453df3</code>
        /// </summary>
        public readonly string ParentId;

        /// <summary>
        /// Unique 64bit identifier for this operation within the trace.
        /// 
        /// <p>Encoded as 16 lowercase hex characters. For example <code>ffdc9bb9a6453df3</code>
        /// 
        /// <p>A span is uniquely identified in storage by (<see cref="TraceId"/>, <see cref="Span.Id"/>).
        /// </summary>
        public readonly string Id;

        /// <summary>
        /// Indicates the primary span type.
        /// </summary>
        public enum SpanKind
        {
            NoKind,
            Client,
            Server,

            /// <summary>
            /// When present, <see cref="Span.Timestamp"/> is the moment a producer sent a message to a destination.
            /// <see cref="Span.Duration"/> represents delay sending the message, such as batching, while <see
            /// cref="Span.RemoteEndpoint"/> indicates the destination, such as a broker.
            ///
            /// <p>Unlike <see cref="Client"/>, messaging spans never share a span ID. For example, the <see cref="Consumer"/>
            /// of the same message has <see cref="Span.ParentId"/> set to this span's <see cref="Span.Id"/>.
            /// </summary>
            Producer,

            /// <summary>
            /// When present, <see cref="Span.Timestamp"/> is the moment a consumer received a message from an
            /// origin. <see cref="Span.Duration"/> represents delay consuming the message, such as from backlog,
            /// while <see cref="Span.RemoteEndpoint"/> indicates the origin, such as a broker.
            ///
            /// <p>Unlike <see cref="Server"/>, messaging spans never share a span ID. For example, the <see cref="Producer"/>
            /// of this message is the <see cref="Span.ParentId"/> of this span.
            /// </summary>
            Consumer
        }

        /// <summary>
        /// When present, used to interpret <see cref="RemoteEndpoint"/>
        /// </summary>
        public readonly SpanKind Kind;

        /// <summary>
        /// Span name in lowercase, rpc method for example.
        /// 
        /// <p>Conventionally, when the span name isn't known, name = "unknown".
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Epoch microseconds of the start of this span, possibly absent if this an incomplete span.
        ///
        /// <p>This value should be set directly by instrumentation, using the most precise value possible.
        ///
        /// <p>There are three known edge-cases where this could be reported absent:
        ///
        /// <pre><ul>
        /// <li>A span was allocated but never started (ex not yet received a timestamp)</li>
        /// <li>The span's start event was lost</li>
        /// <li>Data about a completed span (ex tags) were sent after the fact</li>
        /// </pre><ul>
        ///
        /// <p>Note: timestamps at or before epoch (0L == 1970) are invalid
        ///
        /// <seealso cref="Duration"/>
        /// </summary>
        public readonly DateTime Timestamp;

        /// <summary>
        /// Measurement in microseconds of the critical path, if known. Durations of less than one
        /// microsecond must be rounded up to 1 microsecond.
        /// 
        /// <p>This value should be set directly, as opposed to implicitly via annotation timestamps. Doing
        /// so encourages precision decoupled from problems of clocks, such as skew or NTP updates causing
        /// time to move backwards.
        /// 
        /// <p>If this field is persisted as unset, zipkin will continue to work, except duration query
        /// support will be implementation-specific. Similarly, setting this field non-atomically is
        /// implementation-specific.
        /// 
        /// <p>This field is i64 vs i32 to support spans longer than 35 minutes.
        /// 
        /// </summary>
        public readonly long Duration;

        /// <summary>
        /// The host that recorded this span, primarily for query by service name.
        ///
        /// <p>Instrumentation should always record this and be consistent as possible with the service
        /// name as it is used in search. This is nullable for legacy reasons.
        /// </summary>
        public readonly Endpoint LocalEndpoint;

        /// <summary>
        /// When an RPC (or messaging) span, indicates the other side of the connection.
        ///
        /// <p>By recording the remote endpoint, your trace will contain network context even if the peer
        /// is not tracing. For example, you can record the IP from the <code>X-Forwarded-For</code> header or
        /// the service name and socket of a remote peer.
        /// </summary>
        public readonly Endpoint RemoteEndpoint;

        /// <summary>
        /// Events that explain latency with a timestamp. Unlike log statements, annotations are often
        /// short or contain codes: for example "brave.flush". Annotations are sorted ascending by
        /// timestamp.
        /// </summary>
        public readonly IList<Annotation> Annotations;

        /// <summary>
        /// Tags a span with context, usually to support query or aggregation.
        /// 
        /// <p>For example, a tag key could be <code>"http.path"</code>.
        /// </summary>
        public readonly IDictionary<string, string> Tags;

        /// <summary>
        /// True is a request to store this span even if it overrides sampling policy.
        /// </summary>
        public readonly bool? Debug;

        /// <summary>
        /// True if we are contributing to a span started by another tracer (ex on a different host).
        /// Defaults to null. When set, it is expected for <see cref="Kind"/> to be <see cref="SpanKind.Server"/>.
        /// 
        /// <p>When an RPC trace is client-originated, it will be sampled and the same span ID is used for
        /// the server side. However, the server shouldn't set span.timestamp or duration since it didn't
        /// start the span.
        /// </summary>
        public readonly bool? Shared;

        public readonly string LocalServiceName;

        public readonly string RemoteServiceName;

        private static bool HasFlag(int flags, int flag)
        {
            return (flags & flag) == flag;
        }

        internal Span(Builder builder)
        {
            flags = builder.flags;
            TraceId = builder.traceId;
            ParentId = builder.parentId;
            Id = builder.id;
            Kind = builder.kind;
            Name = builder.name;
            Timestamp = builder.timestamp;
            Duration = builder.duration;
            LocalEndpoint = builder.localEndpoint;
            RemoteEndpoint = builder.remoteEndpoint;
            Annotations = new List<Annotation>(builder.annotations);
            Tags = new Dictionary<string, string>(builder.tags);
            Debug = HasFlag(flags, FLAG_DEBUG_SET) ? HasFlag(flags, FLAG_DEBUG) : (bool?) null;
            Shared = HasFlag(flags, FLAG_SHARED_SET) ? HasFlag(flags, FLAG_SHARED) : (bool?) null;
            LocalServiceName = builder.localEndpoint.ServiceName;
            RemoteServiceName = builder.remoteEndpoint.ServiceName;
        }

        public bool Equals(Span other)
        {
            return string.Equals(TraceId, other.TraceId)
                   && string.Equals(ParentId, other.ParentId)
                   && string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Span && Equals((Span) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = TraceId.GetHashCode();
                hashCode = (hashCode * 397) ^ (ParentId != null ? ParentId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Id != null ? Id.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Span left, Span right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Span left, Span right)
        {
            return !left.Equals(right);
        }

        internal sealed class Builder
        {
            internal string traceId, parentId, id;
            internal SpanKind kind;
            internal string name;
            internal DateTime timestamp;
            internal long duration; // zero means null
            internal Endpoint localEndpoint, remoteEndpoint;
            internal IList<Annotation> annotations = new List<Annotation>();
            internal IDictionary<string, string> tags = new Dictionary<string, string>();
            internal int flags = 0; // bit field for timestamp and duration

            public Builder Reset()
            {
                traceId = null;
                parentId = null;
                id = null;
                kind = default(SpanKind);
                name = null;
                timestamp = default(DateTime);
                duration = 0L;
                localEndpoint = default(Endpoint);
                remoteEndpoint = default(Endpoint);
                annotations?.Clear();
                tags?.Clear();
                flags = 0;
                return this;
            }

            internal Builder()
            {
            }

            internal Builder(Span source)
            {
                traceId = source.TraceId;
                parentId = source.ParentId;
                id = source.Id;
                kind = source.Kind;
                name = source.Name;
                timestamp = source.Timestamp;
                duration = source.Duration;
                localEndpoint = source.LocalEndpoint;
                remoteEndpoint = source.RemoteEndpoint;
                if (source.Annotations.Count != 0)
                {
                    annotations = new List<Annotation>(source.Annotations);
                }

                if (source.Tags.Count != 0)
                {
                    tags = new Dictionary<string, string>(source.Tags);
                }

                flags = source.flags;
            }

            public SpanKind Kind()
            {
                return kind;
            }

            public Endpoint LocalEndpoint()
            {
                return localEndpoint;
            }

            public Builder TraceId(string traceId)
            {
                this.traceId = NormalizeTraceId(traceId);
                return this;
            }

            public Builder ParentId(string parentId)
            {
                if (parentId == null)
                {
                    this.parentId = null;
                    return this;
                }

                var length = parentId.Length;
                if (length > 16) throw new ArgumentException($"{nameof(parentId)}.length > 16");
                ValidateHex(parentId);
                this.parentId = length < 16 ? PadLeft(parentId, 16) : parentId;
                return this;
            }

            public Builder Id(string id)
            {
                if (id == null) throw new ArgumentNullException($"{nameof(id)} == null");
                var length = id.Length;
                if (length > 16) throw new ArgumentException($"{nameof(id)}.length > 16");
                ValidateHex(id);
                this.id = length < 16 ? PadLeft(id, 16) : id;
                return this;
            }

            public Builder Kind(SpanKind kind)
            {
                this.kind = kind;
                return this;
            }

            public Builder Name(string name)
            {
                this.name = string.IsNullOrEmpty(name) ? null : name.ToLower();
                return this;
            }

            public Builder Timestamp(DateTime timestamp)
            {
                this.timestamp = timestamp;
                return this;
            }

            public Builder Duration(long duration)
            {
                if (duration < 0L) duration = 0L;
                this.duration = duration;
                return this;
            }

            public Builder Duration(long? duration)
            {
                return Duration(duration ?? 0L);
            }

            public Builder LocalEndpoint(Endpoint localEndpoint)
            {
                this.localEndpoint = localEndpoint;
                return this;
            }

            public Builder RemoteEndpoint(Endpoint remoteEndpoint)
            {
                this.remoteEndpoint = remoteEndpoint;
                return this;
            }

            public Builder AddAnnotation(DateTime timestamp, string value)
            {
                if (annotations == null) annotations = new List<Annotation>();
                annotations.Add(new Annotation(timestamp, value));
                return this;
            }

            public Builder PutTag(string key, string value)
            {
                if (tags == null) tags = new Dictionary<string, string>();
                if (key == null) throw new ArgumentNullException($"{nameof(key)} == null");
                if (value == null) throw new ArgumentNullException($"{nameof(value)} of " + key + " == null");
                this.tags[key] = value;
                return this;
            }

            public Builder Debug(bool debug)
            {
                flags |= FLAG_DEBUG_SET;
                if (debug)
                {
                    flags |= FLAG_DEBUG;
                }
                else
                {
                    flags &= ~FLAG_DEBUG;
                }

                return this;
            }

            public Builder Debug(bool? debug)
            {
                if (debug.HasValue)
                {
                    return Debug(debug.Value);
                }

                flags &= ~FLAG_DEBUG_SET;
                return this;
            }

            public Builder Shared(bool shared)
            {
                flags |= FLAG_SHARED_SET;
                if (shared)
                {
                    flags |= FLAG_SHARED;
                }
                else
                {
                    flags &= ~FLAG_SHARED;
                }

                return this;
            }

            public Builder Shared(bool? shared)
            {
                if (shared.HasValue)
                {
                    return Shared(shared.Value);
                }

                flags &= ~FLAG_SHARED_SET;
                return this;
            }

            public Span Build()
            {
                var missing = "";
                if (traceId == null) missing += $" {nameof(traceId)}";
                if (id == null) missing += $" {nameof(id)}";
                if (!"".Equals(missing)) throw new InvalidOperationException("Missing :" + missing);
                return new Span(this);
            }
        }

        private static string NormalizeTraceId(string traceId)
        {
            if (traceId == null) throw new ArgumentNullException($"{nameof(traceId)} == null");
            var length = traceId.Length;
            if (length > 32) throw new ArgumentException($"{nameof(traceId)}.length > 32");
            ValidateHex(traceId);
            if (length == 32 || length == 16)
            {
                return traceId;
            }
            else if (length < 16)
            {
                return PadLeft(traceId, 16);
            }
            else
            {
                return PadLeft(traceId, 32);
            }
        }

        private static string PadLeft(string id, int desiredLength)
        {
            return id.PadLeft(desiredLength, '0');
        }

        private static void ValidateHex(string id)
        {
            if (id.Any(c => (c < '0' || c > '9') && (c < 'a' || c > 'f')))
            {
                throw new ArgumentException(id + " should be lower-hex encoded with no prefix");
            }
        }
    }
}