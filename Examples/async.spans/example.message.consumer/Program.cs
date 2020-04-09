using example.message.common;
using System;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
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
                // need to put trae information from producer
                using var messageProducerTrace = new ConsumerTrace("message.consumer", "process message",
                    message.TraceId, message.SpanId, message.ParentId, message.Sampled, message.Flags.ToString(CultureInfo.InvariantCulture));
                await Task.Delay(600); // Test delay for mock processing

                Console.WriteLine($"Message '{message.Text}' was processed!");
            }
            else
            {
                Console.WriteLine($"No messages available!");
            }

            // teardown
            ZipkinHelper.StopZipkin();
            Console.ReadLine();
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
                return JsonSerializer.Deserialize<Message>(content);
        }
    }
}
