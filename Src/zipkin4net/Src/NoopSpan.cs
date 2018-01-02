using System;

namespace zipkin4net
{
    //TODO internal for now. It's still work in progress
    internal class NoopSpan : ISpan
    {
        public ITraceContext Context { get; private set; }

        internal NoopSpan(ITraceContext context)
        {
            Context = context;
        }

        public ISpan Name(string name)
        {
            return this;
        }

        public ISpan Annotate(string value)
        {
            return this;
        }

        public ISpan Annotate(DateTime timestamp, string value)
        {
            return this;
        }

        public ISpan Tag(string key, string value)
        {
            return this;
        }

        public ISpan Kind(SpanKind kind)
        {
            return this;
        }

        public ISpan Start()
        {
            return this;
        }

        public ISpan Start(DateTime timestamp)
        {
            return this;
        }

        public void Finish()
        {
        }

        public void Finish(DateTime timestamp)
        {
        }

        public void Abandon()
        {
        }

        public void Flush()
        {
        }
    }
}