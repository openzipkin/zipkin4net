namespace zipkin4net.Annotation
{
    public sealed class WireRecv : IAnnotation
    {
        internal WireRecv()
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
