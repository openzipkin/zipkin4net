[![Build Status](https://travis-ci.org/criteo/zipkin4net.svg?branch=master)](https://travis-ci.org/criteo/zipkin4net)
# Criteo.Profiling.Tracing

[![Join the chat at https://gitter.im/criteo/zipkin4net](https://badges.gitter.im/criteo/zipkin4net.svg)](https://gitter.im/criteo/zipkin4net?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

A .NET client library for [Zipkin](http://zipkin.io).

## What it provides

It provides you with:
- Zipkin primitives (spans, annotations, binary annotations, ...)
- Asynchronous trace sending
- Trace transport abstraction

## Basic usage

### Bootstrap

The easiest way to use the library is the following

```csharp
var logger = CreateLogger(); //It should implement ILogger
var sender = CreateYourTransport(); // It should implement IZipkinSender

TraceManager.SamplingRate = 1.0f; //full tracing

var tracer = new ZipkinTracer(sender);
TraceManager.RegisterTracer(tracer);
TraceManager.Start(logger);

//Run your application

//On shutdown
TraceManager.Stop();
```

Your zipkin client is now ready!

### Play with traces

To create a new trace, simply call

```csharp
var trace = Trace.Create();
```

Then, you can record annotations

```csharp
trace.Record(Annotations.ServerRecv());
trace.Record(Annotations.ServiceName(serviceName));
trace.Record(Annotations.Rpc("GET"));
trace.Record(Annotations.ServerSend());
trace.Record(Annotations.Tag("http.Get", "<url>")); //adds binary annotation
```

### Transport

The transport is responsible to send traces to a zipkin collector.

#### HTTP transport

We provide you with an [HTTP transport](zipkin4net/Criteo.Profiling.Tracing/Transport/Http/HttpZipkinSender.cs). Just create it with the zipkin collector url (i.e. if you test locally, you'll probably end up with something like 'http://localhost:9411') and pass it to the creation of the [ZipkinTracer](zipkin4net/Criteo.Profiling.Tracing/Tracers/Zipkin/ZipkinTracer.cs) and you're set.

#### Custom transport implementation

The implementation is really easy to do. All you have to do is to implement a Send(byte[]) method.
For example, if you want to send traces through Kafka and assuming you have a kafka producer, you should write

```csharp
class KafkaZipkinSender : IZipkinSender
{
  private readonly IKafkaProducer _producer;
  private const string Topic = "zipkin";

  public KafkaZipkinSender(IKafkaProducer producer)
  {
    _producer = producer;
  }

  public void Send(byte[] data)
  {
    _producer.produce(Topic, data);
  }
}
```

We internally use [Kafka-sharp](https://github.com/criteo/kafka-sharp).

### Span creation

Zipkin is designed to handle complete spans. However, an incorrect usage of the library can lead to incomplete span creation. To create a complete span, you need a pair of matching annotations in the following list:

- ServerRecv and ServerSend annotations
- ClientSend and ClientRecv annotations
- LocalComponentStart and LocalComponentStop annotations

### When are my traces/spans sent?

A span is sent asynchronously to the zipkin collector when one of the following annotation is recorded:
- ServerSend
- ClientRecv
- LocalComponentStop

with the matching opening annotation specified above (matching means same (traceId, spanId, parentSpanId)).

A [flushing mechanism](#flush-mechanism) regurlarly checks for incomplete spans waiting in the memory to be sent.

## Advanced usage

### Monitor the tracer itself

The library comes with few handy tricks to help you monitor tracers

#### Metrics

ZipkinTracer can be created with an implementation of [IStatistics](zipkin4net/Criteo.Profiling.Tracing/Tracers/Zipkin/Statistics.cs) interface. This is useful if you want to send various metrics to graphite.
We update four metrics:

| Metric          | Description                                                             |
| --------------- | ----------------------------------------------------------------------- |
| RecordProcessed | The number of annotation, binaryAnnotations, ... recorded by the tracer |
| SpanSent        | The number of span successfully sent                                    |
| SpanSentBytes   | What went on the network                                                |
| SpanFlushed     | The number of span flushed                                              |

Since the library is meant to have almost no dependency, you'll have to write the implementation that sends these metrics to graphite for example.

#### Tracers for testing and debugging purposes

When integrating this kind of library, it can be useful to know that we called it correctly.

##### Debugging

[ConsoleTracer](zipkin4net/Criteo.Profiling.Tracing/Tracers/ConsoleTracer.cs) writes every record it gets on the console (annotations, service names, ...).

##### Unit/Integration test

- [InMemoryTracer](zipkin4net/Criteo.Profiling.Tracing/Tracers/InMemoryTracer.cs) keeps every record in a queue. It is useful when a mock tracer is not enough.
- [VoidTracer](zipkin4net/Criteo.Profiling.Tracing/Tracers/VoidTracer.cs) drops records. It is useful when you don't need tracing in tests.

Please also note that the default sampling is 0 meaning that by default, tracing is disabled. You should either set the sampling to 1.0 (to be deterministic) or force the trace to be sampled.

### B3 Headers propagation

If your services communicate through HTTP, we provide you with headers injection and extraction ([ZipkinHttpTraceInjector](zipkin4net/Criteo.Profiling.Tracing/Transport/ZipkinHttpTraceInjector.cs) and [ZipkinHttpTraceExtractor](zipkin4net/Criteo.Profiling.Tracing/Transport/ZipkinHttpTraceExtractor.cs)). It will allow you to add headers in HTTP requests between your services.
The headers are zipkin standard headers but you can also implement yours with interfaces [ITraceInjector](zipkin4net/Criteo.Profiling.Tracing/Transport/ITraceInjector.cs) and [ITraceExtractor](zipkin4net/Criteo.Profiling.Tracing/Transport/ITraceExtractor.cs).

### Flush mechanism

We implemented a safety mechanism in the library in order to avoid any memory footprint explosion. Spans can remain in memory if the user doesn't record the matching annotation to complete the span, leading to [a memory leak](#span-creation). To avoid it, we added a safety that removes these spans after 60 seconds.

### Force sampling

You can force a trace to bypass sampling. It is useful for tests but can also be useful if you want to trace specific requests depending on the context.

```csharp
Trace.ForceSampled();
```

If you want to do that in production, we highly recommend too wrap your [IZipkinSender](zipin4net/Criteo.Profiling.Tracing/Tracers/Zipkin/IZipkinSender.cs) implementation with the [RateLimiterZipkinSender](zipkin4net/Criteo.Profiling.Tracing/Tracers/Zipkin/RateLimiterZipkinSender.cs). It will throttle traces based on a time-window pattern.

### Trace context

Passing the trace context accross every API can be very cumbersome and doesn't bring any real added value. It's also a bit painful to implement a working solution if you have async code. That's why we implemented [TraceContext](zipkin4net/Criteo.Profiling.Tracing/TraceContext.cs) which relies on .Net [CallContext](https://msdn.microsoft.com/fr-fr/library/system.runtime.remoting.messaging.callcontext(v=vs.110).aspx) to carry traces over the execution path.

By setting

```csharp
Trace.Current = myTrace;
```

You will be able to retrieve it, even if you have async code in between. It means that it will follow the sync/async path of your request.
Since it can be a bit tricky, please use it with caution and if you know what you're doing.
