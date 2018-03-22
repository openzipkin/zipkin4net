using System;
using System.IO;
using zipkin4net.Internal.Recorder;

namespace zipkin4net.Tracers.Zipkin
{
    internal class ZipkinTracerReporter<S> : IReporter<S>
    {
        private readonly IZipkinSender _sender;
        private readonly ISpanSerializer<S> _spanSerializer;
        private readonly IStatistics _statistics;

        internal ZipkinTracerReporter(IZipkinSender sender, ISpanSerializer<S> spanSerializer, IStatistics statistics)
        {
            if (sender == null)
            {
                throw new ArgumentNullException(nameof(sender),
                    "You have to specify a non-null sender.");
            }

            if (spanSerializer == null)
            {
                throw new ArgumentNullException(nameof(spanSerializer),
                    "You have to specify a non-null span serializer.");
            }

            if (statistics == null)
            {
                throw new ArgumentNullException(nameof(statistics),
                    "You have to specify a non-null statistics.");
            }
            _sender = sender;
            _spanSerializer = spanSerializer;
            _statistics = statistics;
        }

        public void Report(S span)
        {
            byte[] serializedSpan = null;

            using (var memoryStream = new MemoryStream())
            {
                _spanSerializer.SerializeTo(memoryStream, span);
                serializedSpan = memoryStream.ToArray();
            }

            _sender.Send(serializedSpan);
            _statistics.UpdateSpanSent();
            _statistics.UpdateSpanSentBytes(serializedSpan.Length);
        }
    }
}