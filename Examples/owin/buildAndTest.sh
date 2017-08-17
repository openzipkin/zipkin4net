#!/usr/bin/env bash

nuget restore ./owin-example.sln \
&& xbuild /p:Configuration=Release ./owin-example.sln