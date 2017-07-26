using System;
using System.Threading;

namespace zipkin4net.Utils
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

        [ThreadStatic]
        private static Random _localRandom;

        [ThreadStatic]
        private static byte[] _buffer;

        public static long NextLong()
        {
            EnsureInitialized();

            _localRandom.NextBytes(_buffer);
            return BitConverter.ToInt64(_buffer, 0);
        }

        private static void EnsureInitialized()
        {
            if (_localRandom != null)
                return;
            _localRandom = new Random(Interlocked.Increment(ref _seed));
            _buffer = new byte[8];
        }
    }
}
