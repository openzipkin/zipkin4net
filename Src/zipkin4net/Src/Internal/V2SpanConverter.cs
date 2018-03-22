using System;
using System.Text;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Tracers.Zipkin.Thrift;
using zipkin4net.Utils;
using BinaryAnnotation = zipkin4net.Tracers.Zipkin.BinaryAnnotation;
using Span = zipkin4net.Tracers.Zipkin.Span;

namespace zipkin4net.Internal
{
    internal static class V2SpanConverter
    {
        public static Span ToSpan(V2.Span v2Span)
        {
            var traceContext = new SpanState(
                NumberUtils.DecodeHexString(v2Span.TraceId.Substring(0, 16)),
                NumberUtils.DecodeHexString(v2Span.TraceId.Substring(16, 16)),
                v2Span.ParentId != null ? NumberUtils.DecodeHexString(v2Span.ParentId) : (long?)null,
                NumberUtils.DecodeHexString(v2Span.Id),
                true,
                v2Span.Debug ?? false);

            var v1Span = new Span(traceContext, v2Span.Timestamp);
            v1Span.ServiceName = v2Span.LocalServiceName;
            v1Span.Name = v2Span.Name;
            v1Span.Endpoint = v2Span.LocalEndpoint.IpEndPoint;
            v1Span.ServiceName = v2Span.LocalServiceName;

            var startTs = v2Span.Timestamp;
            var endTs = v2Span.Duration == 0L ? default(DateTime) : v2Span.Timestamp.AddMilliseconds((double) v2Span.Duration / 1000.0);
            var kind = v2Span.Kind;
            ZipkinAnnotation
                cs = null, sr = null, ss = null, cr = null, ms = null, mr = null, ws = null, wr = null;
            string remoteEndpointType = null;
            var wroteEndpoint = false;

            foreach (var annotation in v2Span.Annotations)
            {
                var annotationValue = annotation.Value;
                var v1Annotation = new ZipkinAnnotation(annotation.Timestamp, annotationValue);
                if (annotationValue.Length == 2)
                {
                    switch (annotationValue)
                    {
                        case zipkinCoreConstants.CLIENT_SEND:
                            cs = v1Annotation;
                            remoteEndpointType = zipkinCoreConstants.SERVER_ADDR;
                            kind = V2.Span.SpanKind.Client;
                            break;
                        case zipkinCoreConstants.SERVER_RECV:
                            sr = v1Annotation;
                            remoteEndpointType = zipkinCoreConstants.CLIENT_ADDR;
                            kind = V2.Span.SpanKind.Server;
                            break;
                        case zipkinCoreConstants.CLIENT_RECV:
                            kind = V2.Span.SpanKind.Client;
                            cr = v1Annotation;
                            break;
                        case zipkinCoreConstants.MESSAGE_SEND:
                            remoteEndpointType = zipkinCoreConstants.MESSAGE_ADDR;
                            kind = V2.Span.SpanKind.Producer;
                            ms = v1Annotation;
                            break;
                        case zipkinCoreConstants.MESSAGE_RECV:
                            kind = V2.Span.SpanKind.Consumer;
                            mr = v1Annotation;
                            break;
                        case zipkinCoreConstants.WIRE_SEND:
                            ws = v1Annotation;
                            break;
                        case zipkinCoreConstants.WIRE_RECV:
                            wr = v1Annotation;
                            break;
                        default:
                            wroteEndpoint = true;
                            v1Span.AddAnnotation(v1Annotation);
                            break;
                    }
                }
                else
                {
                    wroteEndpoint = true;
                    v1Span.AddAnnotation(v1Annotation);
                }
            }

            if (kind != V2.Span.SpanKind.NoKind)
            {
                switch (kind)
                {
                    case V2.Span.SpanKind.Client:
                        remoteEndpointType = zipkinCoreConstants.SERVER_ADDR;
                        if (startTs != default(DateTime)) cs = new ZipkinAnnotation(startTs, zipkinCoreConstants.CLIENT_SEND);
                        if (endTs != default(DateTime)) cr = new ZipkinAnnotation(endTs, zipkinCoreConstants.CLIENT_RECV);
                        break;
                    case V2.Span.SpanKind.Server:
                        remoteEndpointType = zipkinCoreConstants.CLIENT_ADDR;
                        if (startTs != default(DateTime)) sr = new ZipkinAnnotation(startTs, zipkinCoreConstants.SERVER_RECV);
                        if (endTs != default(DateTime)) ss = new ZipkinAnnotation(endTs, zipkinCoreConstants.SERVER_SEND);
                        break;
                    case V2.Span.SpanKind.Producer:
                        remoteEndpointType = zipkinCoreConstants.MESSAGE_ADDR;
                        if (startTs != default(DateTime)) ms = new ZipkinAnnotation(startTs, zipkinCoreConstants.MESSAGE_SEND);
                        if (endTs != default(DateTime)) ws = new ZipkinAnnotation(endTs, zipkinCoreConstants.WIRE_SEND);
                        break;
                    case V2.Span.SpanKind.Consumer:
                        remoteEndpointType = zipkinCoreConstants.MESSAGE_ADDR;
                        if (startTs != default(DateTime) && endTs != default(DateTime))
                        {
                            wr = new ZipkinAnnotation(startTs, zipkinCoreConstants.WIRE_RECV);
                            mr = new ZipkinAnnotation(endTs, zipkinCoreConstants.MESSAGE_RECV);
                        }
                        else if (startTs != default(DateTime))
                        {
                            mr = new ZipkinAnnotation(startTs, zipkinCoreConstants.MESSAGE_RECV);
                        }

                        break;
                    default:
                        throw new InvalidOperationException("update kind mapping");
                }
            }

            foreach (var tag in v2Span.Tags)
            {
                wroteEndpoint = true;
                var bytes = Encoding.ASCII.GetBytes(tag.Value);
                var binaryAnnotation = new BinaryAnnotation(tag.Key, bytes, AnnotationType.STRING, v2Span.Timestamp, v2Span.LocalServiceName,
                    v2Span.LocalEndpoint.IpEndPoint);
                v1Span.AddBinaryAnnotation(binaryAnnotation);
            }

            if (cs != null
                || sr != null
                || ss != null
                || cr != null
                || ws != null
                || wr != null
                || ms != null
                || mr != null)
            {
                if (cs != null) v1Span.AddAnnotation(cs);
                if (sr != null) v1Span.AddAnnotation(sr);
                if (ss != null) v1Span.AddAnnotation(ss);
                if (cr != null) v1Span.AddAnnotation(cr);
                if (ws != null) v1Span.AddAnnotation(ws);
                if (wr != null) v1Span.AddAnnotation(wr);
                if (ms != null) v1Span.AddAnnotation(ms);
                if (mr != null) v1Span.AddAnnotation(mr);
                wroteEndpoint = true;
            }

            if ((cs != null && cr != null) || (sr != null && ss != null))
            {
                v1Span.SetAsComplete(cr != null ? cr.Timestamp : ss.Timestamp);
            }

            if (remoteEndpointType != null && v2Span.RemoteEndpoint.IpEndPoint != null)
            {
                v1Span.AddBinaryAnnotation(new BinaryAnnotation(remoteEndpointType, BitConverter.GetBytes(true), AnnotationType.STRING, v2Span.Timestamp, v2Span.RemoteServiceName, v2Span.RemoteEndpoint.IpEndPoint));
            }

            if (v2Span.LocalEndpoint.IpEndPoint != null && !wroteEndpoint)
            {
                // create a dummy annotation
                v1Span.AddBinaryAnnotation(new BinaryAnnotation(zipkinCoreConstants.LOCAL_COMPONENT, Encoding.ASCII.GetBytes(""), AnnotationType.STRING, v2Span.Timestamp, v2Span.LocalServiceName, v2Span.LocalEndpoint.IpEndPoint));
                v1Span.SetAsComplete(endTs);
            }

            return v1Span;
        }
    }
}