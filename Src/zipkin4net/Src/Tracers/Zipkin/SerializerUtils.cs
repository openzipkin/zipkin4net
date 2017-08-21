using System;
using System.Linq;
using System.Net;
using zipkin4net.Utils;

namespace zipkin4net.Tracers.Zipkin
{
    public static class SerializerUtils
    {
        /// <summary>
        /// Name of the service when none has been recorded
        /// </summary>
        public const string DefaultServiceName = "UnknownService";
        /// <summary>
        /// Name of the RPC method when none has been recorded
        /// </summary>
        public const string DefaultRpcMethodName = "UnknownRpc";
        /// <summary>
        /// IpEndpoint to use when none has been recorded
        /// </summary>
        public static readonly IPEndPoint DefaultEndPoint = GetLocalEndPointOrDefault();

        public static int IpToInt(IPAddress ipAddr)
        {
            // GetAddressBytes() returns in network order (big-endian)
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(ipAddr.GetAddressBytes(), 0));
        }


        public static string GetServiceNameOrDefault(Span span)
        {
            if (string.IsNullOrWhiteSpace(span.ServiceName))
            {
                // Since we don't have the app name yet, we need to hack a bit by providing
                // an empty service name. This will add the endpoint attribute, and thus enable
                // clock skew correction on the server.
                return IsLocalSpan(span) ? string.Empty : DefaultServiceName;
            }
            return span.ServiceName.Replace(" ", "_"); // whitespaces cause issues with the query and ui
        }

        private static bool IsLocalSpan(Span span)
        {
            return !span.Annotations.Any() &&
                span.BinaryAnnotations.Any(ba => ba.Key == "lc");
        }

        private static IPEndPoint GetLocalEndPointOrDefault()
        {
            IPAddress address;

            try
            {
                // get an ip address and use Loopback if none found
                address = IpUtils.GetLocalIpAddress() ?? IPAddress.Loopback;
            }
            catch (Exception)
            {
                // on failure, use Loopback
                address = IPAddress.Loopback;
            }

            return new IPEndPoint(address, 0);
        }
    }
}
