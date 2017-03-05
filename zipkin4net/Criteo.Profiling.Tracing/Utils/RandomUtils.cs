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
        private static int _seed = Guid.NewGuid().GetHashCode();

        private static readonly ThreadLocal<Random> LocalRandom = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref _seed)));

        public static long NextLong()
        {
            var buffer = new byte[8];
            LocalRandom.Value.NextBytes(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }

    }
}
