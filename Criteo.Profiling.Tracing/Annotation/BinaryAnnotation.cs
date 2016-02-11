using System;

namespace Criteo.Profiling.Tracing.Annotation
{
    public sealed class BinaryAnnotation : IAnnotation
    {
        internal BinaryAnnotation(string key, object value)
        {
            this.Key = key;
            this.Value = value;
        }

        public string Key { get; private set; }
        public object Value { get; private set; }

        public override string ToString()
        {
            return String.Format("{0}: {1} [{2}]", GetType().Name, Key, Value.GetType());
        }

        public void Accept(IAnnotationVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return string.Equals(Key, ((BinaryAnnotation)obj).Key);
        }

        public override int GetHashCode()
        {
            return (Key != null ? Key.GetHashCode() : 0);
        }
    }
}
