namespace zipkin4net.Annotation
{
    public sealed class ConsumerStop : IAnnotation
    {
        internal ConsumerStop()
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
