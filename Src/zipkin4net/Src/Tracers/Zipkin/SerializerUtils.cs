using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
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

        public static string IpToString(IPAddress ipAddr)
        {
            return ipAddr.ToString();
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

        /// <summary>
        /// string value transform to a escaped string
        /// </summary>
        /// <param name="input">original string value</param>
        /// <returns>escaped string value</returns>
        public static string ToEscaped(string input)
        {
            if (input == null)
                return null;
            var literal = new StringBuilder(input.Length + 2);
            foreach (var c in input)
            {
                switch (c)
                {
                    //case '\'': literal.Append(@"\'"); break;
                    case '\"': literal.Append("\\\""); break;
                    case '\\': literal.Append(@"\\"); break;
                    case '\0': literal.Append(@"\0"); break;
                    case '\a': literal.Append(@"\a"); break;
                    case '\b': literal.Append(@"\b"); break;
                    case '\f': literal.Append(@"\f"); break;
                    case '\n': literal.Append(@"\n"); break;
                    case '\r': literal.Append(@"\r"); break;
                    case '\t': literal.Append(@"\t"); break;
                    case '\v': literal.Append(@"\v"); break;
                    default:
                        if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.Control)
                        {
                            literal.Append(c);
                        }
                        else
                        {
                            literal.Append(@"\u");
                            literal.Append(((ushort)c).ToString("x4"));
                        }
                        break;
                }
            }
            return literal.ToString();
        }
    }
}
