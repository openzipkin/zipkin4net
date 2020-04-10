using example.message.common;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using zipkin4net;

namespace example.message.consumer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // setup
            var zipkinServer = args[0];
            ZipkinHelper.StartZipkin(zipkinServer);

            // message fetch
            var message = await FetchAMessage();

            if (message != null)
            {

                await ProcessMessage(message);
                Console.WriteLine($"Message '{message.Text}' was processed!");
            }
            else
            {
                Console.WriteLine($"No messages available!");
            }

            // teardown
            ZipkinHelper.StopZipkin();
            Console.ReadKey();
        }

        static async Task ProcessMessage(Message message)
        {
            // need to supply trace information from producer
            using (var messageProducerTrace = new ConsumerTrace(
                serviceName: "message.consumer",
                rpc: "process message",
                encodedTraceId: message.TraceId,
                encodedSpanId: message.SpanId,
                encodedParentSpanId: message.ParentId,
                sampledStr: message.Sampled,
                flagsStr: message.Flags.ToString(CultureInfo.InvariantCulture)))
            {
                await Task.Delay(600); // Test delay for mock processing
            }
        }

        static async Task<Message> FetchAMessage()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:51589")
            };

            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "messages/pop"));
            var content = response.Content.ReadAsStringAsync().Result;

            if (string.IsNullOrEmpty(content))
                return null;
            else
                return JsonConvert.DeserializeObject<Message>(content);
        }
    }
}
