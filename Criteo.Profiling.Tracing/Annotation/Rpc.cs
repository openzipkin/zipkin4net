using System;

namespace Criteo.Profiling.Tracing.Annotation
{
    public sealed class Rpc : IAnnotation
    {
        public string Name { get; private set; }

        internal Rpc(String name)
        {
            this.Name = name;
        }

        public override string ToString()
        {
            return String.Format("{0}: {1}", GetType().Name, Name);
        }

        public void Accept(IAnnotationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
