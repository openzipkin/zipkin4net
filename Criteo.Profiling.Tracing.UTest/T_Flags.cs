using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest
{
    [TestFixture]
    class T_Flags
    {

        [Test]
        public void EmptyFlagHasNoDebug()
        {
            var flags = Flags.Empty();
            Assert.False(flags.IsDebug());
        }

        [Test]
        public void EmptyFlagDoentKnowSampling()
        {
            var flags = Flags.Empty();
            Assert.False(flags.IsSamplingKnown());
        }

        [Test]
        public void DebugFlagCanBeSet()
        {
            var flagsWithDebug = Flags.Empty().SetDebug();
            Assert.True(flagsWithDebug.IsDebug());
        }

        [Test]
        public void SetSampledLeadsToSamplingKnownAndSampled()
        {
            var flagsWithSampled = Flags.Empty().SetSampled();
            Assert.True(flagsWithSampled.IsSamplingKnown());
            Assert.True(flagsWithSampled.IsSampled());
            Assert.False(flagsWithSampled.IsDebug());
        }

        [Test]
        public void SetNotSampledLeadsToSamplingKnownAndNotSampled()
        {
            var flagsWithNotSampled = Flags.Empty().SetNotSampled();
            Assert.True(flagsWithNotSampled.IsSamplingKnown());
            Assert.False(flagsWithNotSampled.IsSampled());
            Assert.False(flagsWithNotSampled.IsDebug());
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
