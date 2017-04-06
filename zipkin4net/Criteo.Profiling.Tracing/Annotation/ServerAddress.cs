namespace Criteo.Profiling.Tracing.Annotation
{
    public sealed class ServerAddress : IAnnotation
    {
        public string Address { get; private set; }

        internal ServerAddress(string address)
        {
            Address = address;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", GetType().Name, Address);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return string.Equals(Address, ((ServerAddress)obj).Address);
        }

        public override int GetHashCode()
        {
            return Address != null ? Address.GetHashCode() : 0;
        }

        public void Accept(IAnnotationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
