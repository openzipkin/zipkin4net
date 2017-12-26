@echo off
cls

dotnet restore "zipkin4net.dotnetcore.sln"
dotnet build "zipkin4net.dotnetcore.sln" --no-restore
dotnet test "Src/zipkin4net/Tests/zipkin4net.Tests.dotnetcore.csproj" --no-restore --no-build
dotnet test "Src/zipkin4net.middleware.owin/Tests/zipkin4net.middleware.owin.Tests.dotnetcore.csproj" --no-restore --no-build
