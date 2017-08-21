namespace zipkin4net.Annotation
{
    public interface IAnnotation
    {
        void Accept(IAnnotationVisitor visitor);
    }
}
