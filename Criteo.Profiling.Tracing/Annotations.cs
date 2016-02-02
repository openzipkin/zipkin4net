using System.Net;
using Criteo.Profiling.Tracing.Annotation;

namespace Criteo.Profiling.Tracing
{

    /// <summary>
    /// Factory for annotations
    /// </summary>
    public static class Annotations
    {

        public static IAnnotation ClientRecv()
        {
            return new ClientRecv();
        }

        public static IAnnotation ClientSend()
        {
            return new ClientSend();
        }

        public static IAnnotation ServerRecv()
        {
            return new ServerRecv();
        }

        public static IAnnotation ServerSend()
        {
            return new ServerSend();
        }

        public static IAnnotation Rpc(string name)
        {
            return new Rpc(name);
        }

        public static IAnnotation ServiceName(string name)
        {
            return new ServiceName(name);
        }

        public static IAnnotation LocalAddr(IPEndPoint endPoint)
        {
            return new LocalAddr(endPoint);
        }

        public static IAnnotation Binary(string key, object value)
        {
            return new BinaryAnnotation(key, value);
        }

        public static IAnnotation Event(string name)
        {
            return new Event(name);
        }

    }
}
