using System.IO;
using Criteo.Profiling.Tracing.Tracers.Zipkin;
using Criteo.Profiling.Tracing.Utils;

namespace Criteo.Profiling.Tracing.Batcher
{
    public class ZipkinSimpleSpanProcessor : ISpanProcessor
    {
        private readonly IZipkinSender _spanSender;
        private readonly ISpanSerializer _spanSerializer;

        /// <summary>
        /// Serialize and send spans one by one synchronously.
        /// </summary>
        public ZipkinSimpleSpanProcessor(IZipkinSender sender, ISpanSerializer spanSerializer, IStatistics statistics)
        {
            _spanSender = Guard.IsNotNull(sender, nameof(sender));

            _spanSerializer = Guard.IsNotNull(spanSerializer, nameof(spanSerializer));

            Statistics = statistics ?? new Statistics();
        }

        public IStatistics Statistics { get; }

        public void LogSpan(Span spanToLog)
        {
            var memoryStream = new MemoryStream();
            _spanSerializer.SerializeTo(memoryStream, spanToLog);
            byte[] serializedSpans = memoryStream.ToArray();

            _spanSender.Send(serializedSpans);
            Statistics.UpdateSpanSent(1);
            Statistics.UpdateSpanSentBytes(serializedSpans.Length);
        }

    }
}