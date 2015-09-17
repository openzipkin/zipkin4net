using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Criteo.Profiling.Tracing.Utils
{
    internal static class IpUtils
    {

        /// <summary>
        /// Get local IP
        /// (first one which is not loopback if multiple network interfaces are present)
        /// </summary>
        /// <returns>null if not found</returns>
        public static IPAddress GetLocalIpAddress()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                return null;

            var host = Dns.GetHostEntry(Dns.GetHostName());

            return
                host.AddressList.FirstOrDefault(
                    ip => (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip)));
        }

    }
}
