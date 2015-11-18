using System;
using System.Linq;
using Criteo.Profiling.Tracing.Tracers;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest.Tracers
{
    [TestFixture]
    class T_InMemoryTracer
    {

        [Test]
        public void AnnotationsAreCorrectlyRecorded()
        {
            var memoryTracer = new InMemoryTracer();

            var spanId = new SpanId(1, 0, 1, Flags.Empty());

            var rpcAnn = Annotations.Rpc("GET RPC");
            var recordRpc = new Record(spanId, DateTime.UtcNow, rpcAnn);

            var servAnn = Annotations.ServiceName("MyCriteoService");
            var recordServName = new Record(spanId, DateTime.UtcNow, servAnn);

            var servRecv = Annotations.ServerRecv();
            var recordServR = new Record(spanId, DateTime.UtcNow, servRecv);

            var servSend = Annotations.ServerSend();
            var recordServS = new Record(spanId, DateTime.UtcNow, servSend);

            memoryTracer.Record(recordRpc);
            memoryTracer.Record(recordServName);
            memoryTracer.Record(recordServR);
            memoryTracer.Record(recordServS);

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
