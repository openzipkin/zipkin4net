using Microsoft.AspNetCore.Http;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public class Request
    {
        public IHeaderDictionary Headers { get; set; }
        public string Method { get; set; }
    }

    public class HttpContext
    {
        public Request Request { get; set; }
    }

    public interface IApplicationBuilder
    {
        void Use(Action<HttpContext, dynamic> action);
    }
}

namespace Microsoft.AspNetCore.Http
{
    public interface IHeaderDictionary
    {
        string this[string id] { get; }
    }

}
