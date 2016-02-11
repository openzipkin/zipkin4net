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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return string.Equals(Name, ((Rpc)obj).Name, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public void Accept(IAnnotationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
