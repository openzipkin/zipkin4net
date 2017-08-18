using BenchmarkDotNet.Running;
using zipkin4net.Benchmark.Tracers.Zipkin;

namespace zipkin4net.Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<JSONSerializerBenchmark>();
            BenchmarkRunner.Run<ThriftSerializerBenchmark>();
        }
    }
}