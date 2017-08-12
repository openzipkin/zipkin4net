#!/usr/bin/env bash

nuget restore ./zipkin4net-owin.sln
xbuild /p:Configuration=Release ./zipkin4net-owin.sln