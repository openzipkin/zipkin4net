using System.Threading;

namespace Criteo.Profiling.Tracing.Tracers.Zipkin
{
    /// <summary>
    /// Some statistics about the tracing
    /// </summary>
    public interface IStatistics
    {
        /// <summary>
        /// Number of record processed by the tracer
        /// </summary>
        long RecordProcessed { get; }

        /// <summary>
        /// Number of span sent
        /// </summary>
        long SpanSent { get; }

        /// <summary>
        /// Total number of bytes of the sent spans
        /// </summary>
        long SpanSentTotalBytes { get; }

        /// <summary>
        /// Number of span sent after staying
        /// too much time without being completed
        /// </summary>
        long SpanFlushed { get; }

        void UpdateRecordProcessed();

        void UpdateSpanSent(int sent);

        void UpdateSpanFlushed();

        void UpdateSpanSentBytes(int bytesSent);
    }


    public class Statistics : IStatistics
    {
        private long _recordProcessed;
        private long _spanSent;
        private long _spanFlushed;
        private long _spanSentTotalBytes;

        public long RecordProcessed
        {
            get { return _recordProcessed; }
        }

        public long SpanSent
        {
            get { return _spanSent; }
        }

        public long SpanFlushed
        {
            get { return _spanFlushed; }
        }

        public long SpanSentTotalBytes
        {
            get { return _spanSentTotalBytes; }
        }

        public void UpdateRecordProcessed()
        {
            Interlocked.Increment(ref _recordProcessed);
        }

        public void UpdateSpanSent(int sent)
        {
            Interlocked.Add(ref _spanSent,sent);
        }

        public void UpdateSpanFlushed()
        {
            Interlocked.Increment(ref _spanFlushed);
        }

        public void UpdateSpanSentBytes(int bytesSent)
        {
            Interlocked.Add(ref _spanSentTotalBytes, bytesSent);
        }
    }

}
