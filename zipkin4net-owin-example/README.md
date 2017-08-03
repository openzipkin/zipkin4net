# Basic example showing distributed tracing across Owin ASP.Net Web APIs
This is an example app where two Owin ASP.Net web APIs (C#) collaborate on an http request. Notably, timing of these requests are recorded into [Zipkin](http://zipkin.io/), a distributed tracing system. This allows you to see the how long the whole operation took, as well how much time was spent in each service.

Here's an example of what it looks like
![zipkin4net-example](https://files.gitter.im/criteo/zipkin4net/o1To/zipkin4net-example.png)

This example was ported from similar examples, such as [Spring Boot](https://github.com/openzipkin/sleuth-webmvc-example).

# Running the example
This example has two services: frontend and backend. They both report trace data to zipkin. To setup the demo, you need to start Frontend, Backend and Zipkin.

Once the services are started, open http://localhost:8081/
* This will call the backend (http://localhost:9000/api) and show the result, which defaults to printing the current date.

Next, you can view traces that went through the backend via http://localhost:9411/?serviceName=backend
* This is a locally run zipkin service which keeps traces in memory

## Starting the Services

Launch `run.cmd` that will build and start both APIs.

Next, run [Zipkin](http://zipkin.io/), which stores and queries traces reported by the above services.

```bash
wget -O zipkin.jar 'https://search.maven.org/remote_content?g=io.zipkin.java&a=zipkin-server&v=LATEST&c=exec'
java -jar zipkin.jar
```

## Advanced setup

If you want to use different ports that 8081 and 9000 for frontend and backend, you have to do two things:
* Edit the [run file](run.cmd) and change port in urls
* Modify the `callServiceUrl` in the frontend [config file](frontend/App.config) to match the new port as well
