using System;
using Criteo.Profiling.Tracing.Annotation;
using Criteo.Profiling.Tracing.Tracers.Zipkin;
using Criteo.Profiling.Tracing.Utils;
using NUnit.Framework;
using Span = Criteo.Profiling.Tracing.Tracers.Zipkin.Span;

namespace Criteo.Profiling.Tracing.UTest.Tracers.Zipkin
{
    [TestFixture]
    internal class T_Span
    {
        [Test]
        public void DurationAndSpanStartedSetWhenSetAsComplete()
        {
            VerifySpanDurationComputedWhenSetAsComplete(Annotations.ClientSend(), Annotations.ClientRecv(), isRootSpan: false, isSpanStartedAndDurationSet:true);
            VerifySpanDurationComputedWhenSetAsComplete(Annotations.ServerRecv(), Annotations.ServerSend(), isRootSpan: true, isSpanStartedAndDurationSet: true);
            VerifySpanDurationComputedWhenSetAsComplete(Annotations.ServerRecv(), Annotations.ServerSend(), isRootSpan: false, isSpanStartedAndDurationSet: false);
            VerifySpanDurationComputedWhenSetAsComplete(Annotations.LocalOperationStart("Operation"), Annotations.LocalOperationStop(), isRootSpan: false, isSpanStartedAndDurationSet: true);
        }

        private static void VerifySpanDurationComputedWhenSetAsComplete(IAnnotation start, IAnnotation stop, bool isRootSpan, bool isSpanStartedAndDurationSet)
        {
            var startTime = DateTime.Now;
            var endTime = startTime.AddHours(1);
            var expectedDuration = endTime.Subtract(startTime);

            long? parentId = 0;
            if (isRootSpan)
                parentId = null;
            var spanState = new SpanState(1, parentId, 2, SpanFlags.None);
            var spanCreatedTimestamp = TimeUtils.UtcNow;
            var span = new Span(spanState, spanCreatedTimestamp);

            var recordStart = new Record(spanState, startTime, start);
            var visitorStart = new ZipkinAnnotationVisitor(recordStart, span);
            var recordStop  = new Record(spanState, endTime, stop);
            var visitorStop  = new ZipkinAnnotationVisitor(recordStop, span);

            Assert.AreEqual(spanCreatedTimestamp, span.SpanCreated);
            Assert.False(span.Duration.HasValue);
            Assert.False(span.Complete);
            recordStart.Annotation.Accept(visitorStart);
            Assert.False(span.Duration.HasValue);
            Assert.False(span.Complete);
            recordStop.Annotation.Accept(visitorStop);
            Assert.True(span.Complete);
            if (isSpanStartedAndDurationSet)
            {
                Assert.AreEqual(expectedDuration, span.Duration);
                Assert.AreEqual(startTime, span.SpanStarted);
            }
            else
            {
                Assert.False(span.Duration.HasValue);
                Assert.False(span.SpanStarted.HasValue);
            }
        }

        [Test]
        public void ClientDurationIsPreferredOverServer()
        {
            var spanState = new SpanState(1, 0, 2, SpanFlags.None);
            var span = new Span(spanState, TimeUtils.UtcNow);
            const int offset = 10;

            var annotationTime = TimeUtils.UtcNow;
            var recordServerRecv = new Record(spanState, annotationTime, Annotations.ServerRecv());
            recordServerRecv.Annotation.Accept(new ZipkinAnnotationVisitor(recordServerRecv, span));
            var recordServerSend = new Record(spanState, annotationTime.AddMilliseconds(offset), Annotations.ServerSend());
            recordServerSend.Annotation.Accept(new ZipkinAnnotationVisitor(recordServerSend, span));
            var recordClientSend = new Record(spanState, annotationTime.AddMilliseconds(-offset), Annotations.ClientSend());
            recordClientSend.Annotation.Accept(new ZipkinAnnotationVisitor(recordClientSend, span));
            var recordClientRecv = new Record(spanState, annotationTime.AddMilliseconds(2 * offset), Annotations.ClientRecv());
            recordClientRecv.Annotation.Accept(new ZipkinAnnotationVisitor(recordClientRecv, span));

            Assert.True(span.Duration.HasValue);
            Assert.AreEqual(3 * offset, span.Duration.Value.TotalMilliseconds);
        }

        [TestCase(-200, false)]
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(200, true)]
        public void SpanHasDurationOnlyIfValueIsPositive(int offset, bool shouldHaveDuration)
        {
            var duration = GetSpanDuration(offset, Annotations.ClientSend(), Annotations.ClientRecv());
            if (shouldHaveDuration)
            {
                Assert.AreEqual(offset, (int) duration.Value.TotalMilliseconds);
            }
            else
            {
                Assert.False(duration.HasValue);
            }
        }

        [Test]
        public void SpanDoesntHaveDurationIfIncomplete()
        {
            const int offset = 10;

            Assert.False(GetSpanDuration(offset, Annotations.ServerRecv()).HasValue);
            Assert.False(GetSpanDuration(offset, Annotations.ServerSend()).HasValue);
            Assert.False(GetSpanDuration(offset, Annotations.ClientRecv()).HasValue);
            Assert.False(GetSpanDuration(offset, Annotations.ClientSend()).HasValue);
            Assert.False(GetSpanDuration(offset, Annotations.LocalOperationStart("Operation")).HasValue);
            Assert.False(GetSpanDuration(offset, Annotations.LocalOperationStop()).HasValue);
        }


        private static TimeSpan? GetSpanDuration(int offset, IAnnotation firstAnnotation, IAnnotation secondAnnotation = null)
        {
            var spanState = new SpanState(1, 0, 2, SpanFlags.None);
            var span = new Span(spanState, TimeUtils.UtcNow);

            var annotationTime = TimeUtils.UtcNow;
            var firstRecord = new Record(spanState, annotationTime, firstAnnotation);
            firstRecord.Annotation.Accept(new ZipkinAnnotationVisitor(firstRecord, span));
            if (secondAnnotation != null)
            {
                var secondRecord = new Record(spanState, annotationTime.AddMilliseconds(offset), secondAnnotation);
                secondRecord.Annotation.Accept(new ZipkinAnnotationVisitor(secondRecord, span));
            }

            return span.Duration;
        }
    }
}
