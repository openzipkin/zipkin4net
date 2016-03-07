using Criteo.Profiling.Tracing.Tracers.Zipkin;
using Criteo.Profiling.Tracing.Tracers.Zipkin.Thrift;
using Criteo.Profiling.Tracing.Utils;
using NUnit.Framework;
using Span = Criteo.Profiling.Tracing.Tracers.Zipkin.Span;

namespace Criteo.Profiling.Tracing.UTest.Tracers.Zipkin
{
    [TestFixture]
    class T_Span
    {

        [Test]
        [Description("Span should only be marked as complete when either ClientRecv or ServerSend are present.")]
        public void SpansAreLabeledAsCompleteWhenCrOrSs()
        {
            var spanState = new SpanState(1, 0, 2, SpanFlags.None);
            var started = TimeUtils.UtcNow;

            var spanClientRecv = new Span(spanState, started);
            Assert.False(spanClientRecv.Complete);

            spanClientRecv.AddAnnotation(new ZipkinAnnotation(TimeUtils.UtcNow, zipkinCoreConstants.CLIENT_RECV));
            Assert.True(spanClientRecv.Complete);

            var spanServSend = new Span(spanState, started);
            Assert.False(spanServSend.Complete);

            spanServSend.AddAnnotation(new ZipkinAnnotation(TimeUtils.UtcNow, zipkinCoreConstants.SERVER_SEND));
            Assert.True(spanServSend.Complete);


            var spanOtherAnn = new Span(spanState, started);
            Assert.False(spanOtherAnn.Complete);

            spanServSend.AddAnnotation(new ZipkinAnnotation(TimeUtils.UtcNow, zipkinCoreConstants.SERVER_RECV));
            spanServSend.AddAnnotation(new ZipkinAnnotation(TimeUtils.UtcNow, zipkinCoreConstants.CLIENT_SEND));
            Assert.False(spanOtherAnn.Complete);
        }

    }
}
