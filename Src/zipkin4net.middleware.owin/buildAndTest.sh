#!/usr/bin/env bash

nuget restore ./zipkin4net.middleware.owin.sln \
&& xbuild /p:Configuration=Release ./zipkin4net.middleware.owin.sln \
&& mono packages/NUnit.Runners.2.6.4/tools/nunit-console.exe Tests/bin/Release/zipkin4net.middleware.owin.Tests.dll