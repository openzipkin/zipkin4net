``` ini

BenchmarkDotNet=v0.10.3.0, OS=OSX
Processor=Intel(R) Core(TM) i5-5287U CPU 2.90GHz, ProcessorCount=4
Frequency=1000000000 Hz, Resolution=1.0000 ns, Timer=UNKNOWN
dotnet cli version=1.0.0-preview2-003131
  [Host]     : .NET Core 4.6.25009.03, 64bit RyuJIT
  DefaultJob : .NET Core 4.6.24628.01, 64bit RyuJIT


```
 |                                                  Method |      Mean |    StdDev |
 |-------------------------------------------------------- |---------- |---------- |
 |                      writeLocalSpanJSONWithCustomParser | 1.4042 us | 0.0165 us |
 |                     writeClientSpanJSONWithCustomParser | 1.4692 us | 0.0170 us |
 |                     writeServerSpanJSONWithCustomParser | 1.4303 us | 0.0189 us |
 |       writeLocalSpanJSONWithJSONDotNetParserWithoutList | 2.6782 us | 0.0293 us |
 |      writeClientSpanJSONWithJSONDotNetParserWithoutList | 2.6764 us | 0.0323 us |
 |      writeServerSpanJSONWithJSONDotNetParserWithoutList | 2.6316 us | 0.0319 us |
 |          writeLocalSpanJSONWithJSONDotNetParserWithList | 3.0389 us | 0.0307 us |
 |         writeClientSpanJSONWithJSONDotNetParserWithList | 3.0641 us | 0.0394 us |
 |         writeServerSpanJSONWithJSONDotNetParserWithList | 3.0670 us | 0.0565 us |
 |  writeLocalSpanJSONWithJSONDotNetParserWithListCreation | 3.1453 us | 0.0263 us |
 | writeClientSpanJSONWithJSONDotNetParserWithListCreation | 3.1033 us | 0.0583 us |
 | writeServerSpanJSONWithJSONDotNetParserWithListCreation | 3.1990 us | 0.0491 us |

``` ini

BenchmarkDotNet=v0.10.3.0, OS=OSX
Processor=Intel(R) Core(TM) i5-5287U CPU 2.90GHz, ProcessorCount=4
Frequency=1000000000 Hz, Resolution=1.0000 ns, Timer=UNKNOWN
dotnet cli version=1.0.0-preview2-003131
  [Host]     : .NET Core 4.6.25009.03, 64bit RyuJIT
  DefaultJob : .NET Core 4.6.24628.01, 64bit RyuJIT


```
 |                Method |      Mean |    StdDev |
 |---------------------- |---------- |---------- |
 |  writeLocalSpanThrift | 1.0028 us | 0.0141 us |
 | writeClientSpanThrift | 1.0334 us | 0.0213 us |
 | writeServerSpanThrift | 1.0320 us | 0.0235 us |
