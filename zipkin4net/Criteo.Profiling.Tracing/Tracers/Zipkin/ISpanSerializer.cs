﻿using System.Collections.Generic;
using System.IO;

namespace Criteo.Profiling.Tracing.Tracers.Zipkin
{
    public interface ISpanSerializer
    {
        void SerializeTo(Stream stream, Span span);
        void SerializeTo(Stream stream, IEnumerable<Span> spans);
    }
}