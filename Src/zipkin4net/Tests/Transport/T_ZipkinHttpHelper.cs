using zipkin4net.Transport;
using NUnit.Framework;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;


namespace zipkin4net.UTests.Transport
{
	[TestFixture]
	internal class T_ZipkinHttpHelper
	{
		[Test]
		[TestCase("X-B3-TraceId", "Test_TraceID_123")]
        [TestCase("", "Test_TraceID_123")]
        [TestCase("", "")]
        public void ExtractorCorrectlyExtractsExistingHeader(string key, string value)
		{
			var carrier = new HttpRequestMessage().Headers;
			carrier.Add(key, value);

			Assert.AreEqual(ZipkinHttpHelper.ExtractorHelper(carrier, key), value);
		}

		[Test]
        [TestCase("X-B3-TraceId")]
        [TestCase("")]
        public void ExtractorCorrectlyExtractsNonExistentHeader(string key)
		{
            var carrier = new HttpRequestMessage().Headers;
            Assert.AreEqual(ZipkinHttpHelper.ExtractorHelper(carrier, key), string.Empty);
        }

		// No test needed for Injector

	}
}

