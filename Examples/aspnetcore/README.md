# Basic example showing distributed tracing across ASP.Net core apps
This is an example app where two ASP.Net core (C#) services collaborate on an http request. Notably, timing of these requests are recorded into [Zipkin](http://zipkin.io/), a distributed tracing system. This allows you to see the how long the whole operation took, as well how much time was spent in each service.

Here's an example of what it looks like
![zipkin4net-example](https://files.gitter.im/criteo/zipkin4net/o1To/zipkin4net-example.png)

This example was ported from similar examples, such as [Spring Boot](https://github.com/openzipkin/sleuth-webmvc-example).

# Implementation Overview

Web requests are served by Kestrel, and tracing is automatically performed by [zipkin4net](https://github.com/criteo/zipkin4net), more specifically by the library [zipkin4net](/Src/zipkin4net/Src).

# Pre-requisites

In order to build the example, you need to install a two things:
- [dotnet 1.0.0 preview2 3131](https://github.com/dotnet/core/blob/master/release-notes/download-archives/1.0.1-preview2-download.md)
- [mono](http://www.mono-project.com/download/) if you're on linux or OS X

# Running the example
This example has two services: frontend and backend. They both report trace data to zipkin. To setup the demo, you need to start Frontend, Backend and Zipkin.

Once the services are started, open http://localhost:8081/
* This will call the backend (http://localhost:9000/api) and show the result, which defaults to printing the current date.

Next, you can view traces that went through the backend via http://localhost:9411/?serviceName=backend
* This is a locally run zipkin service which keeps traces in memory

## Starting the Services
First, we need to compile the example
```bash
$ pwd
~/zipkin4net/
$ ./buildAndTest.sh
```

In separate tabs or windows, start each of frontend and backend:
```bash
$ pwd
~/zipkin4net/zipkin4net-example/frontend
$ dotnet run http://*:8081
```
and
```bash
$ pwd
~/zipkin4net/zipkin4net-example/backend
$ dotnet run http://*:9000
```


Next, run [Zipkin](http://zipkin.io/), which stores and queries traces reported by the above services.

```bash
wget -O zipkin.jar 'https://search.maven.org/remote_content?g=io.zipkin.java&a=zipkin-server&v=LATEST&c=exec'
java -jar zipkin.jar
```

## Advanced setup

If you want to use different ports that 8081 and 9000 for frontend and backend, you have to do two things:
* Edit the [configuration](/Examples/aspnetcore/frontend/appSettings.json) and change the callServiceUrl to the desired backend url
* Launch frontend and backend with the ports you want (the parameter just after dotnet run)
