using Criteo.Profiling.Tracing.Utils;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest.Utils
{
    [TestFixture]
    class T_NumberUtils
    {

        [Test]
        public void TransformationIsReversible()
        {
            const long longId = 150L;

            var guid = NumberUtils.LongToGuid(longId);
            var backToLongId = NumberUtils.GuidToLong(guid);

            Assert.AreEqual(longId, backToLongId);
        }

    }
}
