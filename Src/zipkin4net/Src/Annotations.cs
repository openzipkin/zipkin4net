using System.Net;
using zipkin4net.Annotation;

namespace zipkin4net
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
        private static readonly IAnnotation AnnWireSend = new WireSend();
        private static readonly IAnnotation AnnWireRecv = new WireRecv();
        private static readonly IAnnotation AnnProducerStart = new ProducerStart();
        private static readonly IAnnotation AnnProducerStop = new ProducerStop();
        private static readonly IAnnotation AnnConsumerStart = new ConsumerStart();
        private static readonly IAnnotation AnnConsumerStop = new ConsumerStop();
        private static readonly IAnnotation AnnLocalOperationStop = new LocalOperationStop();

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

        public static IAnnotation WireSend()
        {
            return AnnWireSend;
        }

        public static IAnnotation WireRecv()
        {
            return AnnWireRecv;
        }

        public static IAnnotation ProducerStart()
        {
            return AnnProducerStart;
        }

        public static IAnnotation ProducerStop()
        {
            return AnnProducerStop;
        }

        public static IAnnotation ConsumerStart()
        {
            return AnnConsumerStart;
        }

        public static IAnnotation ConsumerStop()
        {
            return AnnConsumerStop;
        }

        public static IAnnotation MessageAddr(string serviceName, IPEndPoint endPoint)
        {
            return new MessageAddr(serviceName, endPoint);
        }

        public static IAnnotation LocalOperationStart(string name)
        {
            return new LocalOperationStart(name);
        }

        public static IAnnotation LocalOperationStop()
        {
            return AnnLocalOperationStop;
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

        public static IAnnotation ClientAddr(IPEndPoint ipEndPoint)
        {
            return new ClientAddr(ipEndPoint);
        }

        public static IAnnotation ServerAddr(string serviceName, IPEndPoint ipEndPoint)
        {
            return new ServerAddr(serviceName, ipEndPoint);
        }
    }
}
