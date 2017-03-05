namespace Criteo.Profiling.Tracing.Annotation
{
    public interface IAnnotation
    {
        void Accept(IAnnotationVisitor visitor);
    }
}
