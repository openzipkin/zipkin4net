using Criteo.Profiling.Tracing.Annotation;
using System;

namespace Criteo.Profiling.Tracing.Middleware.Tests.Helpers
{
    static class CheckHelper
    {
        internal static Func<string, string, TagAnnotation, bool> has = (key, value, tagAnnotation) =>
            tagAnnotation.Key == key &&
            tagAnnotation.Value.ToString().Equals(value);
    }
}
