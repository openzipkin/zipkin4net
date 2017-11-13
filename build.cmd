@echo off
cls

dotnet restore "zipkin4net.dotnetcore.sln"
dotnet build "zipkin4net.dotnetcore.sln"
dotnet test "Src/zipkin4net/Tests/zipkin4net.Tests.dotnetcore.csproj"
dotnet test "Src/zipkin4net.middleware.owin/Tests/zipkin4net.middleware.owin.Tests.dotnetcore.csproj"
