using System.Net;
using Criteo.Profiling.Tracing.Logger;
using Criteo.Profiling.Tracing.Utils;

namespace Criteo.Profiling.Tracing
{
    public class Configuration
    {

        /// <summary>
        /// Name of the RPC method when none has been recorded
        /// </summary>
        public string DefaultRpcMethodName = "UnknownRpc";

        /// <summary>
        /// Name of the service when none has been recorded
        /// </summary>
        public string DefaultServiceName = "UnknownService";

        /// <summary>
        /// IpEndpoint to use when none has been recorded
        /// </summary>
        public IPEndPoint DefaultEndPoint = new IPEndPoint(IpUtils.GetLocalIpAddress() ?? IPAddress.Loopback, 0);

        /// <summary>
        /// Logger to record events inside the library
        /// </summary>
        public ILogger Logger = new VoidLogger();

    }
}
