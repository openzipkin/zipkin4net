namespace zipkin4net.Annotation
{
    public sealed class ServerRecv : IAnnotation
    {
        internal ServerRecv()
        {
        }

        public override string ToString()
        {
            return GetType().Name;
        }

        public void Accept(IAnnotationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
