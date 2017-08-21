using zipkin4net.Tracers.Zipkin.Thrift;

namespace zipkin4net.Annotation
{
    public static class CommonTags
    {
        public static readonly string HttpStatusCode = zipkinCoreConstants.HTTP_STATUS_CODE;
        public static readonly string HttpMethod = zipkinCoreConstants.HTTP_METHOD;
        public const string HostName = "host.name";
        public static readonly string Error = zipkinCoreConstants.ERROR;
    }
}
