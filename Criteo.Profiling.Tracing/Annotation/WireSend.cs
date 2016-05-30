namespace Criteo.Profiling.Tracing.Annotation
{
    public sealed class WireSend : IAnnotation
    {
        internal WireSend()
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
