namespace zipkin4net.Annotation
{
    public sealed class ClientSend : IAnnotation
    {

        internal ClientSend()
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
