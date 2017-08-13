#!/usr/bin/env bash

nuget restore ./zipkin4net-owin.sln
xbuild /p:Configuration=Release ./zipkin4net-owin.sln
mono packages/NUnit.Runners.2.6.4/tools/nunit-console.exe Criteo.Profiling.Tracing.Middleware.Tests/bin/Release/Criteo.Profiling.Tracing.Middleware.Tests.dll