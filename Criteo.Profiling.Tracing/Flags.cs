using System.Diagnostics.Contracts;

namespace Criteo.Profiling.Tracing
{

    /// <summary>
    /// Represents flags that can be passed along in request headers.
    /// C# version of Finagle Flags class (com.twitter.finagle.tracing.Flags).
    /// </summary>
    public struct Flags
    {
        private const long DebugMask = 1L << 0;
        private const long SamplingSetMask = 1L << 1;
        private const long SampledMask = 1L << 2;

        private readonly long _value;

        public static readonly Flags Empty = new Flags(0L);

        private Flags(long flags)
        {
            _value = flags;
        }

        public static Flags FromLong(long value)
        {
            return new Flags(value);
        }

        public bool IsFlagSet(long mask)
        {
            return (_value & mask) == mask;
        }

        [Pure]
        public Flags SetFlag(long mask)
        {
            return new Flags(_value | mask);
        }

        public bool IsDebug()
        {
            return IsFlagSet(DebugMask);
        }

        [Pure]
        public Flags SetDebug()
        {
            return SetFlag(DebugMask);
        }

        [Pure]
        public Flags SetSampled()
        {
            return SetFlag(SamplingSetMask).SetFlag(SampledMask);
        }

        [Pure]
        public Flags SetNotSampled()
        {
            return SetFlag(SamplingSetMask);
        }

        public bool IsSampled()
        {
            return IsFlagSet(SampledMask);
        }

        public bool IsSamplingKnown()
        {
            return IsFlagSet(SamplingSetMask);
        }

        public long ToLong()
        {
            return _value;
        }
    }
}
