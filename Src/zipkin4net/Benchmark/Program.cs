using BenchmarkDotNet.Running;
using Criteo.Profiling.Tracing.Benchmark.Tracers.Zipkin;

namespace Criteo.Profiling.Tracing.Benchmark
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