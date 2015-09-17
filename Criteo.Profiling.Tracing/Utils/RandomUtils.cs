using System;
using System.Threading;

namespace Criteo.Profiling.Tracing.Utils
{
    /// <summary>
    /// Thread-safe random long generator.
    ///
    /// See "Correct way to use Random in multithread application"
    /// http://stackoverflow.com/questions/19270507/correct-way-to-use-random-in-multithread-application
    /// </summary>
    internal static class RandomUtils
    {
        private static int seed = Environment.TickCount;

        private static readonly ThreadLocal<Random> rand = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

        public static long NextLong()
        {
            return (long)((rand.Value.NextDouble() * 2.0 - 1.0) * long.MaxValue);
        }

    }
}
