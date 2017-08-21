namespace zipkin4net.Annotation
{
    public sealed class ClientRecv : IAnnotation
    {
        internal ClientRecv()
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
