@echo off
cls

dotnet restore "zipkin4net.sln"
dotnet build "zipkin4net.sln" --no-restore
dotnet test "Src/zipkin4net/Tests/zipkin4net.Tests.csproj" --no-restore --no-build
dotnet test "Src/zipkin4net.middleware.owin/Tests/zipkin4net.middleware.owin.Tests.csproj" --no-restore --no-build
