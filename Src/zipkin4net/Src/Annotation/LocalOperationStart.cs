namespace zipkin4net.Annotation
{
    public sealed class LocalOperationStart : IAnnotation
    {
        public string OperationName { get; private set; }

        internal LocalOperationStart(string operationName)
        {
            OperationName = operationName;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", GetType().Name, OperationName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return string.Equals(OperationName, ((LocalOperationStart)obj).OperationName);
        }

        public override int GetHashCode()
        {
            return OperationName != null ? OperationName.GetHashCode() : 0;
        }

        public void Accept(IAnnotationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
