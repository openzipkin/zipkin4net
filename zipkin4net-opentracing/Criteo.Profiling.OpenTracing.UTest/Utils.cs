using System;
using Criteo.Profiling.Tracing;
using Criteo.Profiling.Tracing.Annotation;
using Criteo.Profiling.Tracing.Tracers;
using NUnit.Framework;

static internal class Utils
{
    internal static void AssertNextAnnotationIs(InMemoryTracer tracer, IAnnotation annotation, DateTime timestamp = default(DateTime))
    {
        Record record = null;
        if (!tracer.Records.TryDequeue(out record))
        {
            Assert.Fail("Trying to get next record but doesn't exist");
        }

        Assert.AreEqual(annotation, record.Annotation);
        if (!timestamp.Equals(default(DateTime)))
        {
            Assert.AreEqual(timestamp, record.Timestamp);
        }
    }
}