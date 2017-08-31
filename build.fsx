open System
#r "build-packages/FAKE.4.63.0/tools/FakeLib.dll" // include Fake lib
open Fake

type ProjectAndFramework = {Path : string ; Framework : string}

type Buildable = 
    | Project of ProjectAndFramework
    | Solution of string

let outputFolder = __SOURCE_DIRECTORY__ </> "bin"
let dotnetExePath = "dotnet"
let nugetVersion = "1.0.0"

let processTimeout = TimeSpan.FromMinutes 5.

Target "Clean" (fun _ ->
    let extractBinAndObjFromPath =
        System.IO.Path.GetDirectoryName
        >> fun p -> [p </> "bin" ; p </> "obj"]

    !! "Src/**/*.csproj"
    |> Seq.map extractBinAndObjFromPath
    |> (Seq.collect id >> Seq.distinct >> Seq.toList)
    |> fun paths -> outputFolder :: paths
    |> CleanDirs
)

let dotnetcoreTestProject = {Path = "Src/zipkin4net/Tests/zipkin4net.Tests.dotnetcore.csproj" ; Framework = "netcoreapp2.0"}

let dotnetcoreZipkin4netProject = {Path = "Src/zipkin4net/Src/zipkin4net.dotnetcore.csproj" ; Framework = "netstandard1.5"}

let dotnetcoreBuildProjects : Buildable list = [
    Project dotnetcoreZipkin4netProject
    Project {Path = "Src/zipkin4net/Benchmark/zipkin4net.Benchmark.dotnetcore.csproj" ; Framework = "netcoreapp2.0"}
    Project dotnetcoreTestProject
    Solution "Src/zipkin4net.middleware.aspnetcore/zipkin4net.middleware.aspnetcore.dotnetcore.sln"
]

let buildDotnetcore : Buildable seq -> unit = 
    Seq.iter (fun buildable -> 
            match buildable with
            | Project proj ->
                DotNetCli.Build (fun c ->
                    { c with
                        Project = proj.Path
                        ToolPath = dotnetExePath
                        AdditionalArgs = [ "-f " + proj.Framework ]
                    })
            | Solution path ->
                DotNetCli.Build (fun c ->
                    { c with
                        Project = path
                        ToolPath = dotnetExePath
                    })
)

let raiseWhenNot0 message status =
    match status with 
    | 0 -> ()
    | _ -> failwith message

let build buildTool = 
    let buildFromPath path = 
        ExecProcess (fun info ->
            info.FileName <- buildTool
            info.Arguments <- sprintf "/p:Configuration=Release %s" path
            ) processTimeout |> (raiseWhenNot0 "Build failed")
    function
    | Solution p -> buildFromPath p
    | Project { Path = p } -> buildFromPath p

let owinSolution = Solution "Src/zipkin4net.middleware.owin/zipkin4net.middleware.owin.sln"

Target "Build" (fun _ ->
    dotnetcoreBuildProjects
    |> buildDotnetcore
    
    match isWindows with
    | true -> build "msbuild" owinSolution
    | _ -> build "xbuild" owinSolution
)

let nugetRestore = 
    let restore path = 
        ExecProcess (fun info ->
            info.FileName <- "nuget"
            info.Arguments <- sprintf "restore %s" path
            ) processTimeout |> (raiseWhenNot0 "Restore failed")
    function
    | Solution p -> restore p
    | Project { Path = p } -> restore p


Target "Restore" (fun _ ->
    nugetRestore (Solution "Src/zipkin4net/zipkin4net.sln")
    nugetRestore owinSolution
)

let testDotnetcoreProject : ProjectAndFramework seq -> unit = 
    Seq.iter (fun proj ->
        DotNetCli.Test (fun c ->
            { c with
                Project = proj.Path
                ToolPath = dotnetExePath
                AdditionalArgs = [ "-f " + proj.Framework ; "--no-build" ]
            })
)

let dotnetcoreTestProjects = [
    dotnetcoreTestProject
]

Target "Test" (fun _ ->
    dotnetcoreTestProjects
    |> testDotnetcoreProject
    
    let fileName = 
        match isWindows with
        | true -> "Src/zipkin4net.middleware.owin/packages/NUnit.Runners.2.6.4/tools/nunit-console.exe"
        | _ -> "mono Src/zipkin4net.middleware.owin/packages/NUnit.Runners.2.6.4/tools/nunit-console.exe"
    ExecProcess (fun info ->
        info.FileName <- fileName
        info.Arguments <- sprintf "Src/zipkin4net.middleware.owin/Tests/bin/Release/zipkin4net.middleware.owin.Tests.dll"
        ) processTimeout |> (raiseWhenNot0 "Test failed")
)

let dotnetcorePackProjects = [
    dotnetcoreZipkin4netProject
]

Target "DotnetcorePackage" (fun _ ->
    dotnetcorePackProjects
    |> Seq.iter (fun proj ->
        DotNetCli.Pack (fun c ->
            { c with
                Project = proj.Path
                ToolPath = dotnetExePath
                AdditionalArgs = [(sprintf "-o %s" outputFolder) ; (sprintf "/p:Version=%s" nugetVersion)]
            })
    )
)

"Clean"
    ==> "Restore"
    ==> "Build"
    ==> "Test"
    ==> "DotnetcorePackage"

RunTargetOrDefault "Test"