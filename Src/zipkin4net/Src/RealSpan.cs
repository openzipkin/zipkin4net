using System;
using zipkin4net.Internal.Recorder;
using zipkin4net.Utils;

namespace zipkin4net
{
    //TODO: internal for now. It's still work in progress
    internal class RealSpan : ISpan
    {
        private readonly IRecorder _recorder;

        public ITraceContext Context { get; private set; }

        public RealSpan(ITraceContext context, IRecorder recorder)
        {
            Context = context;
            this._recorder = recorder;
        }

        public ISpan Name(string name)
        {
            _recorder.Name(Context, name);
            return this;
        }

        public ISpan Annotate(string value)
        {
            return Annotate(TimeUtils.UtcNow, value);
        }

        public ISpan Annotate(DateTime timestamp, string value)
        {
            _recorder.Annotate(Context, timestamp, value);
            return this;
        }

        public ISpan Tag(string key, string value)
        {
            _recorder.Tag(Context, key, value);
            return this;
        }


        public ISpan Kind(SpanKind kind)
        {
            _recorder.Kind(Context, kind);
            return this;
        }

        public ISpan Start()
        {
            return Start(TimeUtils.UtcNow);
        }

        public ISpan Start(DateTime timestamp)
        {
            _recorder.Start(Context, timestamp);
            return this;
        }

        public void Finish()
        {
            Finish(TimeUtils.UtcNow);
        }

        public void Finish(DateTime timestamp)
        {
            _recorder.Finish(Context, timestamp);
        }

        public void Abandon()
        {
            _recorder.Abandon(Context);
        }

        public void Flush()
        {
            _recorder.Flush(Context);
        }
    }
}