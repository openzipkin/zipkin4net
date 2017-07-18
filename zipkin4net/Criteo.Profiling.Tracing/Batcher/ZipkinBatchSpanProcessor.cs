using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Criteo.Profiling.Tracing.Tracers.Zipkin;
using Criteo.Profiling.Tracing.Utils;

namespace Criteo.Profiling.Tracing.Batcher
{
    public class ZipkinBatchSpanProcessor : ISpanProcessor
    {
        private readonly int _batchSize;
        private readonly ManualResetEventSlim _eventSlim = new ManualResetEventSlim(false, 1);
        private readonly int _maxCapacity = 1000;
        private readonly Task _processQueueTask;
        private readonly ConcurrentQueue<Span> _queue = new ConcurrentQueue<Span>();
        private readonly List<Span> _spanBucket = new List<Span>();
        private readonly IZipkinSender _spanSender;
        private readonly ISpanSerializer _spanSerializer;
        private readonly TimeSpan _timeWindow;

        /// <summary>
        /// Serialize and send spans in batches of fixed size asynchronously.
        /// </summary>
        public ZipkinBatchSpanProcessor(IZipkinSender sender, ISpanSerializer spanSerializer, IStatistics statistics,
            TimeSpan timeWindow, int batchSize = 10)
        {
            _spanSender = Guard.IsNotNull(sender, nameof(sender));

            _spanSerializer = Guard.IsNotNull(spanSerializer, nameof(spanSerializer));

            Statistics = Guard.IsNotNull(statistics, nameof(statistics));

            if (batchSize < 1)
                throw new ArgumentException("Batch size must be greater than 0", "batchSize");
            _batchSize = batchSize;

            if (timeWindow <= TimeSpan.Zero)
                throw new ArgumentException("Time window must be positive", "timeWindow");
            _timeWindow = timeWindow;

            _processQueueTask = Task.Factory.StartNew(Consumer, CancellationToken.None, TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        public IStatistics Statistics { get; }

        #region public

        public void LogSpan(Span spanToLog)
        {
            if (ShouldEnqueueSpan())
            {
                _queue.Enqueue(spanToLog);
                if (!_eventSlim.IsSet)
                    _eventSlim.Set();
            }
        }

        #endregion public

        #region helper

        private void SendSpan(IEnumerable<Span> spans)
        {
            if (!spans.Any())
                return;
            var memoryStream = new MemoryStream();
            _spanSerializer.SerializeTo(memoryStream, spans);
            byte[] serializedSpan = memoryStream.ToArray();

            _spanSender.Send(serializedSpan);
            Statistics.UpdateSpanSent(spans.Count());
            Statistics.UpdateSpanSentBytes(serializedSpan.Length);
        }

        #endregion helper


        #region queueing

        private bool ShouldEnqueueSpan()
        {
            return _queue.Count < _maxCapacity;
        }

        private void Consumer()
        {
            while (true)
                if (_eventSlim.Wait(_timeWindow))
                {
                    ProcessQueue();
                    _eventSlim.Reset();
                }
                else
                {
                    Flush();
                }
        }

        private void ProcessQueue()
        {
            Span span;
            while (_queue.TryDequeue(out span))
            {
                Add(span);
                if (_spanBucket.Count >= _batchSize)
                    Flush();
            }
        }

        #endregion queuing

        #region batching

        protected void Add(Span span)
        {
            _spanBucket.Add(span);
        }

        private void Flush()
        {
            SendSpan(_spanBucket);
            _spanBucket.Clear();
        }

        #endregion batching
    }
}