using System.Net;
using Criteo.Profiling.Tracing.Annotation;

namespace Criteo.Profiling.Tracing
{

    /// <summary>
    /// Factory for annotations
    /// </summary>
    public static class Annotations
    {
        private static readonly IAnnotation AnnClientReceive = new ClientRecv();
        private static readonly IAnnotation AnnClientSend = new ClientSend();
        private static readonly IAnnotation AnnServerReceive = new ServerRecv();
        private static readonly IAnnotation AnnServerSend = new ServerSend();

        public static IAnnotation ClientRecv()
        {
            return AnnClientReceive;
        }

        public static IAnnotation ClientSend()
        {
            return AnnClientSend;
        }

        public static IAnnotation ServerRecv()
        {
            return AnnServerReceive;
        }

        public static IAnnotation ServerSend()
        {
            return AnnServerSend;
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

        public static IAnnotation Tag(string key, string value)
        {
            return new TagAnnotation(key, value);
        }

        public static IAnnotation Event(string name)
        {
            return new Event(name);
        }

    }
}
