using example.message.common;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using zipkin4net;
using zipkin4net.Propagation;

namespace example.message.producer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // setup
            var zipkinServer = args[0];
            ZipkinHelper.StartZipkin(zipkinServer);

            // message sending
            var text = Guid.NewGuid().ToString();
            // need to create current trace before using ProducerTrace
            Trace.Current = Trace.Create();
            using (var messageProducerTrace = new ProducerTrace("message.producer", "create message"))
            {
                await ProduceMessage(messageProducerTrace.Trace.CurrentSpan, text);
            }
            Console.WriteLine($"Message '{text}' sent to message center!");

            // teardown
            ZipkinHelper.StopZipkin();
            Console.ReadLine();
        }

        static async Task ProduceMessage(ITraceContext traceContext, string text)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:51589")
            };

            var message = new Message
            {
                Text = text,
                TraceId = traceContext.SerializeTraceId(),
                SpanId = traceContext.SerializeSpanId(),
                Sampled = traceContext.SerializeSampledKey(),
                Flags = long.Parse(traceContext.SerializeDebugKey())
            };

            await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "messages/push")
            {
                Content = new StringContent(JsonSerializer.Serialize(message), Encoding.UTF8, "application/json")
            }); ;
        }
    }
}
