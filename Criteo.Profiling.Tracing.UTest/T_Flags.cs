using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest
{
    [TestFixture]
    class T_Flags
    {

        [Test]
        public void EmptyFlagHasNoDebugNorSampling()
        {
            var flags = Flags.Empty();

            Assert.False(flags.IsDebug());
            Assert.False(flags.IsSamplingKnown());
            Assert.AreEqual(0, flags.ToLong());
        }

        [Test]
        public void DebugFlagCanBeSet()
        {
            var flagsWithDebug = Flags.Empty().SetDebug();

            Assert.True(flagsWithDebug.IsDebug());
            Assert.AreEqual(1, flagsWithDebug.ToLong());
        }

        [Test]
        public void SetSampledLeadsToSamplingKnownAndSampled()
        {
            var flagsWithSampled = Flags.Empty().SetSampled();

            Assert.True(flagsWithSampled.IsSamplingKnown());
            Assert.True(flagsWithSampled.IsSampled());
            Assert.False(flagsWithSampled.IsDebug());
            Assert.AreEqual(6, flagsWithSampled.ToLong());
        }

        [Test]
        public void SetNotSampledLeadsToSamplingKnownAndNotSampled()
        {
            var flagsWithNotSampled = Flags.Empty().SetNotSampled();

            Assert.True(flagsWithNotSampled.IsSamplingKnown());
            Assert.False(flagsWithNotSampled.IsSampled());
            Assert.False(flagsWithNotSampled.IsDebug());
            Assert.AreEqual(2, flagsWithNotSampled.ToLong());
        }

        [Test]
        public void FlagsRecreatedEqualsToOriginal()
        {
            var originalFlags = Flags.Empty().SetDebug().SetSampled();

            var flagsAsLong = originalFlags.ToLong();

            var reconstructedFlags = Flags.FromLong(flagsAsLong);

            Assert.AreEqual(originalFlags, reconstructedFlags);
        }

    }
}
