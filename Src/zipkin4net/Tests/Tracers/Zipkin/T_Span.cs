using System;
using System.Text;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Utils;
using NUnit.Framework;
using zipkin4net.Tracers.Zipkin.Thrift;
using BinaryAnnotation = zipkin4net.Tracers.Zipkin.BinaryAnnotation;
using Span = zipkin4net.Tracers.Zipkin.Span;

namespace zipkin4net.UTest.Tracers.Zipkin
{
    [TestFixture]
    internal class T_Span
    {
        [Test]
        public void MinimumDurationShouldBeAMicrosecond()
        {
            var spanState = new SpanState(1, null, 2, isSampled: null, isDebug: false);
            var span = new Span(spanState, TimeUtils.UtcNow);
            var annotationTime = span.SpanCreated;
            span.AddBinaryAnnotation(new BinaryAnnotation(zipkinCoreConstants.LOCAL_COMPONENT,
                Encoding.UTF8.GetBytes("lc1"), AnnotationType.STRING, annotationTime,
                SerializerUtils.DefaultServiceName, SerializerUtils.DefaultEndPoint));
            span.SetAsComplete(annotationTime.AddTicks(1));

            Assert.NotNull(span.Duration);
            Assert.AreEqual(0.001, span.Duration.Value.TotalMilliseconds);
        }

        [TestCase(-200, false)]
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(200, true)]
        public void SpanHasDurationOnlyIfValueIsPositive(int offset, bool shouldHaveDuration)
        {
            var duration = GetSpanDuration(TimeSpan.FromMilliseconds(offset), zipkinCoreConstants.CLIENT_SEND,
                zipkinCoreConstants.CLIENT_RECV);
            if (shouldHaveDuration)
            {
                Assert.NotNull(duration);
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
            var offset = TimeSpan.FromMilliseconds(10);

            Assert.False(GetSpanDuration(offset, zipkinCoreConstants.SERVER_SEND).HasValue);
            Assert.False(GetSpanDuration(offset, zipkinCoreConstants.CLIENT_RECV).HasValue);
        }


        private static TimeSpan? GetSpanDuration(TimeSpan offset, params string[] annotations)
        {
            var spanState = new SpanState(1, null, 2, isSampled: null, isDebug: false);
            var span = new Span(spanState, TimeUtils.UtcNow);
            var annotationTime = span.SpanCreated;

            Array.ForEach(annotations, a =>
                span.AddAnnotation(new ZipkinAnnotation(annotationTime, a)));

            span.SetAsComplete(annotationTime.Add(offset));

            return span.Duration;
        }
    }
}