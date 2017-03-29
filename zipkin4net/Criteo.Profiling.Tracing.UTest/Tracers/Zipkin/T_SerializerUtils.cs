using System.Net;
using NUnit.Framework;
using Criteo.Profiling.Tracing.Tracers.Zipkin;

namespace Criteo.Profiling.Tracing.UTest.Tracers.Zipkin
{
    [TestFixture]
    internal class T_SerializerUtils
    {
        [Test]
        public void IpToIntConversionIsCorrect()
        {
            const string ipStr = "192.168.1.56";
            const int expectedIp = unchecked((int)3232235832);

            var ipAddr = IPAddress.Parse(ipStr);

            var ipInt = SerializerUtils.IpToInt(ipAddr);

            Assert.AreEqual(expectedIp, ipInt);
        }
    }
}