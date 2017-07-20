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
src="Criteo.Profiling.Tracing/Criteo.Profiling.Tracing.dotnetcore.csproj"
tests="Criteo.Profiling.Tracing.UTest/Criteo.Profiling.Tracing.UTest.dotnetcore.csproj"
benchmark="Criteo.Profiling.Tracing.Benchmark/Criteo.Profiling.Tracing.Benchmark.dotnetcore.csproj"

check_availability "dotnet"

dotnet restore  $solution   \
&& dotnet build -f "netstandard1.5" $src         \
&& dotnet build -f "netcoreapp1.1" $benchmark         \
&& dotnet build -f "netcoreapp1.0" $tests        \
&& dotnet test -f "netcoreapp1.0" $tests           \