#r "build-packages/FAKE.4.63.0/tools/FakeLib.dll" // include Fake lib
open Fake

type ProjectAndFramework = {Project : string ; Framework : string}
let outputFoler = "bin"
let dotnetExePath = "dotnet"
let nugetVersion = "1.0.0"

Target "Clean" (fun _ ->
    !! "Src/**/*.csproj"
    |> Seq.map (System.IO.Path.GetDirectoryName)
    |> Seq.map (fun p -> [p </> "bin" ; p </> "obj"])
    |> Seq.collect id
    |> Seq.toList
    |> fun paths -> outputFoler :: paths
    |> CleanDirs
)

Target "DotnetRestoreTools" (fun _ ->
    DotNetCli.Restore (fun c ->
        { c with
            Project = "Src/zipkin4net/zipkin4net.dotnetcore.sln"
            ToolPath = dotnetExePath 
        })
)

let dotnetTestProject = {Project = "Src/zipkin4net/Tests/zipkin4net.Tests.dotnetcore.csproj" ; Framework = "netcoreapp1.0"}

let buildProjects = [
    {Project = "Src/zipkin4net/Src/zipkin4net.dotnetcore.csproj" ; Framework = "netstandard1.5"}
    {Project = "Src/zipkin4net/Benchmark/zipkin4net.Benchmark.dotnetcore.csproj" ; Framework = "netcoreapp1.1"}
    dotnetTestProject
]

Target "DotnetBuild" (fun _ ->
    buildProjects
    |> Seq.iter (fun proj ->
        DotNetCli.Build (fun c ->
            { c with
                Project = proj.Project
                ToolPath = dotnetExePath
                AdditionalArgs = [ "-f " + proj.Framework ]
            })
    )
)

let testProjects = [
    dotnetTestProject
]

Target "DotnetTest" (fun _ ->
    testProjects
    |> Seq.iter (fun proj ->
        DotNetCli.Build (fun c ->
            { c with
                Project = proj.Project
                ToolPath = dotnetExePath
                AdditionalArgs = [ "-f " + proj.Framework ]
            })
    )
)

let packProjects = [
    {Project = "Src/zipkin4net/Src/zipkin4net.dotnetcore.csproj" ; Framework = "netstandard1.5"}
]

Target "DotnetPackage" (fun _ ->
    packProjects
    |> Seq.iter (fun proj ->
        DotNetCli.Pack (fun c ->
            { c with
                Project = proj.Project
                ToolPath = dotnetExePath
                AdditionalArgs = [(sprintf "-o %s" outputFoler) ; (sprintf "/p:Version=%s" nugetVersion)]
            })
    )
)

"Clean"
    ==> "DotnetRestoreTools"
    ==> "DotnetBuild"
    ==> "DotnetTest"
    ==> "DotnetPackage"

RunTargetOrDefault "DotnetPackage"