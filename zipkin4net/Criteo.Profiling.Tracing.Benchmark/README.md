``` ini

BenchmarkDotNet=v0.10.3.0, OS=OSX
Processor=Intel(R) Core(TM) i5-5287U CPU 2.90GHz, ProcessorCount=4
Frequency=1000000000 Hz, Resolution=1.0000 ns, Timer=UNKNOWN
dotnet cli version=1.0.0-preview2-003131
  [Host]     : .NET Core 4.6.25009.03, 64bit RyuJIT
  DefaultJob : .NET Core 4.6.24628.01, 64bit RyuJIT


```
 |                              Method |      Mean |    StdDev |
 |------------------------------------ |---------- |---------- |
 |  writeLocalSpanJSONWithCustomParser | 2.2808 us | 0.0159 us |
 | writeClientSpanJSONWithCustomParser | 2.2924 us | 0.0443 us |
 | writeServerSpanJSONWithCustomParser | 2.2982 us | 0.0818 us |
