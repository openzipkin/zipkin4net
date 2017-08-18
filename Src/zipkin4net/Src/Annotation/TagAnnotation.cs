namespace zipkin4net.Annotation
{
    public sealed class TagAnnotation : IAnnotation
    {
        internal TagAnnotation(string key, object value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; private set; }
        public object Value { get; private set; }

        public override string ToString()
        {
            return string.Format("{0}: {1} [{2}]", GetType().Name, Key, Value.GetType());
        }

        public void Accept(IAnnotationVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return string.Equals(Key, ((TagAnnotation)obj).Key);
        }

        public override int GetHashCode()
        {
            return Key != null ? Key.GetHashCode() : 0;
        }
    }
}
