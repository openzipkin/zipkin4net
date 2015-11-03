using System.Linq;
using Criteo.Profiling.Tracing.Tracers;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest.Tracers
{
    [TestFixture]
    class T_InMemoryTracer
    {

        [SetUp]
        public void EnableAndClearTracers()
        {
            Trace.TracingEnabled = true;
            Tracer.Clear();
        }

        [Test]
        public void AnnotationsAreCorrectlyRecorded()
        {
            var memoryTracer = new InMemoryTracer();
            Tracer.Register(memoryTracer);
            Trace.SamplingRate = 1f;
            var trace = Trace.CreateIfSampled();

            var rpcAnn = Annotations.Rpc("GET RPC");
            var servAnn = Annotations.ServiceName("MyCriteoService");
            var servRecv = Annotations.ServerRecv();
            var servSend = Annotations.ServerSend();

            trace.Record(rpcAnn).Wait();
            trace.Record(servAnn).Wait();
            trace.Record(servRecv).Wait();
            trace.Record(servSend).Wait();

            var records = memoryTracer.Records.ToList();
            var annotations = records.Select(record => record.Annotation).ToList();

            Assert.AreEqual(4, annotations.Count());

            Assert.True(annotations.Contains(rpcAnn));
            Assert.True(annotations.Contains(servAnn));
            Assert.True(annotations.Contains(servRecv));
            Assert.True(annotations.Contains(servSend));
        }
    }
}
