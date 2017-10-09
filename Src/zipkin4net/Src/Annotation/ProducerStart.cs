namespace zipkin4net.Annotation
{
    public sealed class ProducerStart : IAnnotation
    {
        internal ProducerStart()
        {}

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
