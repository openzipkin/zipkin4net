using System;
using System.Linq;
using zipkin4net.Tracers;
using zipkin4net.Utils;
using NUnit.Framework;

namespace zipkin4net.UTest.Tracers
{
    [TestFixture]
    internal class T_InMemoryTracer
    {

        [Test]
        public void AnnotationsAreCorrectlyRecorded()
        {
            var memoryTracer = new InMemoryTracer();

            var spanState = new SpanState(1, 0, 1, isSampled: null, isDebug: false);

            var rpcAnn = Annotations.Rpc("GET RPC");
            var recordRpc = new Record(spanState, TimeUtils.UtcNow, rpcAnn);

            var servAnn = Annotations.ServiceName("MyCriteoService");
            var recordServName = new Record(spanState, TimeUtils.UtcNow, servAnn);

            var servRecv = Annotations.ServerRecv();
            var recordServR = new Record(spanState, TimeUtils.UtcNow, servRecv);

            var servSend = Annotations.ServerSend();
            var recordServS = new Record(spanState, TimeUtils.UtcNow, servSend);

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
