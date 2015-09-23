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

            var trace = Trace.Create();

            var rpcAnn = Annotations.Rpc("GET RPC");
            var servAnn = Annotations.ServiceName("MyCriteoService");
            var servRecv = Annotations.ServerRecv();
            var servSend = Annotations.ServerSend();

            trace.Record(rpcAnn);
            trace.Record(servAnn);
            trace.Record(servRecv);
            trace.Record(servSend);

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
