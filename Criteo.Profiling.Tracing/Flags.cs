using System;

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

        private readonly long value;

        private Flags(long flags)
        {
            this.value = flags;
        }

        public static Flags Empty()
        {
            return new Flags(0L);
        }

        public static Flags FromLong(long value)
        {
            return new Flags(value);
        }

        public bool IsFlagSet(long mask)
        {
            return (value & mask) == mask;
        }

        public Flags SetFlag(long mask)
        {
            return new Flags(value | mask);
        }

        public bool IsDebug()
        {
            return IsFlagSet(DebugMask);
        }

        public Flags SetDebug()
        {
            return SetFlag(DebugMask);
        }

        public Flags SetSampled()
        {
            return SetFlag(SamplingSetMask).SetFlag(SampledMask);
        }

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
            return value;
        }
    }
}
