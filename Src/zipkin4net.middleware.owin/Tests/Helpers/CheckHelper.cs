using zipkin4net.Annotation;
using System;

namespace zipkin4net.Middleware.Tests.Helpers
{
    static class CheckHelper
    {
        internal static Func<string, string, TagAnnotation, bool> has = (key, value, tagAnnotation) =>
            tagAnnotation.Key == key &&
            tagAnnotation.Value.ToString().Equals(value);
    }
}
