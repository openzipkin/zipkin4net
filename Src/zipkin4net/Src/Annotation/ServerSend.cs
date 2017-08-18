namespace zipkin4net.Annotation
{
    public sealed class ServerSend : IAnnotation
    {
        internal ServerSend()
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
