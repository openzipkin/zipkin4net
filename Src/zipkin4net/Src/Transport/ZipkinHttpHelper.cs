using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;

namespace zipkin4net.Transport
{
	public static class ZipkinHttpHelper
	{
        public static void InjectorHelper(HttpClient carrier, string key, string value)
        {
            carrier.DefaultRequestHeaders.Add(key, value);
        }

        public static string ExtractorHelper(HttpRequestHeaders carrier, string key)
        {
            IEnumerable<string> headerValues;
            string header = string.Empty;
            if (carrier.TryGetValues(key, out headerValues))
            {
                header = headerValues.FirstOrDefault();
            }

            return header;
        }
    }
}

