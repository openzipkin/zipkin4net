#!/usr/bin/env bash

function check_availability() {
    binary=$1
    which $binary 2>&1 > /dev/null
    if [ $? -ne 0 ]; then
	echo "$binary could not be found in PATH"
        exit 1
    fi
}

solution="zipkin4net.dotnetcore.sln"
tests="Criteo.Profiling.Tracing.UTest/Criteo.Profiling.Tracing.UTest.dotnetcore.csproj"

check_availability "dotnet"

dotnet restore  $solution   \
&& dotnet build $solution   \
&& dotnet test $tests       \