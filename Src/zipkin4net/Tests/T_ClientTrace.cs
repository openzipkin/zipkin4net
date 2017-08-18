using zipkin4net;
using NUnit.Framework;

namespace zipkin4net.UTest
{
    [TestFixture]
    internal class T_ClientTrace
    {
        private const string serviceName = "service1";
        private const string rpc = "rpc";

        [Test]
        public void ShouldNotSetCurrentTrace()
        {
            Trace.Current = null;
            using (var client = new ClientTrace(serviceName, rpc))
            {
                Assert.IsNull(client.Trace);
            }
        }

        [Test]
        public void ShouldCallChildWhenCurrentTraceNotNull()
        {
            var trace = Trace.Create();
            Trace.Current = trace;
            using (var client = new ClientTrace(serviceName, rpc))
            {
                Assert.AreEqual(trace.CurrentSpan.SpanId, client.Trace.CurrentSpan.ParentSpanId);
                Assert.AreEqual(trace.CurrentSpan.TraceId, client.Trace.CurrentSpan.TraceId);
            }
        }
    }
}