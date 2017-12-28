dotnet restore zipkin4net.sln
msbuild zipkin4net.sln
dotnet test -f netcoreapp2.0 Src/zipkin4net/Tests/zipkin4net.Tests.csproj
