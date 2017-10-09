namespace zipkin4net.Annotation
{
    public sealed class ConsumerStart : IAnnotation
    {
        internal ConsumerStart()
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
