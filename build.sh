dotnet restore zipkin4net.dotnetcore.sln
msbuild zipkin4net.dotnetcore.sln
dotnet test -f netcoreapp2.0 Src/zipkin4net/Tests/zipkin4net.Tests.dotnetcore.csproj
