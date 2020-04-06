using zipkin4net.Annotation;
using System;

namespace zipkin4net
{
    public class BaseStandardTrace
    {
        public virtual Trace Trace { internal set;  get; }

        public void AddAnnotation(IAnnotation annotation)
        {
            Trace.Record(annotation);
        }

        public virtual void Error(Exception ex)
        {
            Trace.Record(Annotations.Tag("error", ex.Message));
        }
    }
}
