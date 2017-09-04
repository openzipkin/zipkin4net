open System
#r "build-packages/FAKE.4.63.0/tools/FakeLib.dll" // include Fake lib
open Fake

type Path = string
type ProjectAndFramework = {Path : Path ; FrameworkVersion : string}
type IsTestProject = bool
type Buildable = 
    | Project of Project
    | Solution of Solution
and Project = 
    | CoreProject of ProjectAndFramework * IsTestProject
    | ClassicProject of Path * IsTestProject
    member __.Path = 
        match __ with
        | CoreProject (p, _) -> p.Path
        | ClassicProject (p, _) -> p
    member __.IsTest = 
        match __ with
        | CoreProject (_, isTest) -> isTest
        | ClassicProject (_, isTest) -> isTest
and Solution =
    | CoreSolution of Path
    | ClassicSolution of Path

let outputFolder = __SOURCE_DIRECTORY__ </> "bin"
let dotnetExePath = "dotnet"
let nugetVersion = "1.0.0"
let nunitConsolePath = __SOURCE_DIRECTORY__ </> "build-packages/NUnit.ConsoleRunner.3.7.0/tools/nunit3-console.exe"
let processTimeout = TimeSpan.FromMinutes 5.

let versionMap = [("zipkin4net.dotnetcore.csproj", "netstandard1.5") ; ("common.dotnetcore.csproj", "netstandard2.0")] |> Map
let classifyProject (path:string) = 
    let getDotnetCoreVersion (path:string) =
        let specificVersion = Map.tryFind (System.IO.Path.GetFileName path) versionMap
        match specificVersion with
        | None -> "netcoreapp2.0"
        | Some v -> v
    let isTest (path:string) = path.Contains("Tests")
    if path.Contains("dotnetcore") then CoreProject ({Path = path ; FrameworkVersion = getDotnetCoreVersion path }, isTest path)
    else ClassicProject (path, isTest path)

let classifySolution (path:string) = 
    if path.Contains("dotnetcore") then CoreSolution path
    else ClassicSolution path
let projects =
    !! "**/*.csproj" 
    |> Seq.map classifyProject
    |> Seq.toList

let solutions =
    !! "**/*.sln" |> Seq.toList
    |> Seq.map classifySolution
    |> Seq.toList

let buildables = 
    projects
    |> Seq.map (Project)
    |> Seq.append (Seq.map (Solution) solutions)
    |> Seq.toList

// -----------------------------------------------------------------------------------

let extractBinAndObjFromPath =
    System.IO.Path.GetDirectoryName
    >> fun p -> [p </> "bin" ; p </> "obj"]

Target "Clean" (fun _ ->
    projects
    |> Seq.map (fun project -> extractBinAndObjFromPath project.Path)
    |> (Seq.collect id >> Seq.distinct >> Seq.toList)
    |> fun paths -> outputFolder :: paths
    |> CleanDirs
)

let buildCoreProject (coreProject : ProjectAndFramework) = 
    DotNetCli.Build (fun c ->
        { c with
            Project = coreProject.Path
            ToolPath = dotnetExePath
            AdditionalArgs = [ "-f " + coreProject.FrameworkVersion ]
        })

let buildCoreSolution (path : Path) = 
    DotNetCli.Build (fun c ->
        { c with
            Project = path
            ToolPath = dotnetExePath
        })

let raiseWhenNot0 message status =
    match status with 
    | 0 -> ()
    | _ -> failwith message

let buildClassic buildTool path = 
    ExecProcess (fun info ->
        info.FileName <- buildTool
        info.Arguments <- sprintf "/p:Configuration=Release %s" path
        ) processTimeout |> (raiseWhenNot0 "Build failed")

let buildClassicWithTool = if isWindows then (buildClassic "msbuild") else (buildClassic "xbuild")

let build = 
    function
    | Project (CoreProject (projectAndFramework, _)) -> buildCoreProject projectAndFramework
    | Project (ClassicProject (path, _)) -> buildClassicWithTool path
    | Solution (CoreSolution path) -> buildCoreSolution path
    | Solution (ClassicSolution path) -> buildClassicWithTool path

Target "Build" (fun _ ->
    if isWindows then
        solutions |> List.iter (Solution >> build)
    else 
        let excluded = ["zipkin4net.Tests.csproj"] |> Set
        projects |> List.filter (fun p -> not (excluded.Contains(p.Path))) |> List.iter (Project >> build)
)

let nugetRestore path = 
    ExecProcess (fun info ->
        info.FileName <- "nuget"
        info.Arguments <- sprintf "restore %s" path
        ) processTimeout |> (raiseWhenNot0 "Restore failed")

let restore = 
    function
    | ClassicSolution path -> nugetRestore path
    | _ -> ()

Target "Restore" (fun _ ->
    solutions
    |> List.iter restore
)

let testCoreProject project = 
    DotNetCli.Test (fun c ->
        { c with
            Project = project.Path
            ToolPath = dotnetExePath
            AdditionalArgs = [ "-f " + project.FrameworkVersion ; "--no-build" ]
        })

let testWithNunit dllPath = 
    let fileName, args = 
        match isWindows with
        | true -> nunitConsolePath, dllPath
        | _ -> "mono", sprintf "%s %s" nunitConsolePath dllPath

    ExecProcess (fun info ->
        info.FileName <- fileName
        info.Arguments <- args
        ) processTimeout |> (raiseWhenNot0 "Test failed")

let tryGetProjectDll path = 
    System.IO.Path.GetFileNameWithoutExtension path
    |> sprintf "**/%s.dll"
    |> (!!)
    |> Seq.tryHead

let test = 
    function
    | CoreProject (projectAndFramework, _) -> testCoreProject projectAndFramework
    | ClassicProject (path, _) ->
         match tryGetProjectDll path with
         | None -> invalidOp (sprintf "Cannot find test dll for project: %s" path)
         | Some dll -> testWithNunit dll

Target "Test" (fun _ ->
    projects
    |> Seq.filter (fun p -> p.IsTest)
    |> Seq.iter test
)

let corePack path = 
    DotNetCli.Pack (fun c ->
        { c with
            Project = path
            ToolPath = dotnetExePath
            AdditionalArgs = [(sprintf "-o %s" outputFolder) ; (sprintf "/p:Version=%s" nugetVersion)]
        })

Target "Pack" (fun _ ->
    "Src/zipkin4net/Src/zipkin4net.dotnetcore.csproj"
    |> corePack
)

"Clean"
    ==> "Restore"
    ==> "Build"
    ==> "Test"
    ==> "Pack"

RunTargetOrDefault "Test"