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
            _recorder = recorder;
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

        public bool IsNoop()
        {
            return false;
        }

        public ISpan Start()
        {
            _recorder.Start(Context);
            return this;
        }

        public void Finish()
        {
            _recorder.Finish(Context);
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