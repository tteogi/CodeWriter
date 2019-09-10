#r "paket:
nuget Fake.Core.ReleaseNotes
nuget Fake.Core.Xml
nuget Fake.DotNet.Cli
nuget Fake.DotNet.Paket
nuget Fake.Tools.Git
nuget Fake.Core.Process
nuget Fake.IO.FileSystem
nuget Fake.DotNet.MSBuild
nuget Fake.DotNet.NuGet
nuget Fake.Core.Target //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.IO
open Fake.Core.TargetOperators
open System
open System.IO
open Fake.DotNet
open Fake.DotNet.NuGet


let nugetApiKye = "oy2ejb6lhkzh7d2zav5atjfiocskyyeuxdazii2znduk3e"
let buildDir = "./build/"
let testDir  = "./test/"
let configuration = DotNet.BuildConfiguration.Release


// Properties
let summary = "Helper class for generating code concisely."
let copyright = "Copyright © 2019 tteogi"
let description = """
Helper class for generating code concisely.
This package converts to .Net core from CodeWriter (https://github.com/SaladLab/CodeWriter).
"""
let authors = ["Ten Y"]
let owner = "Ten Y"
let tags = "code write builder generator"
let solutionFile = "CodeWriter"
let nugetVersion = "1.0.1";
let gitHome = "https://github.com/tteogi"
let gitName = "CodeWriter"
let project = "CodeWriter.Core"
let projectUrl = sprintf "%s/%s" gitHome gitName
let licenceUrl = "https://raw.githubusercontent.com/tteogi/tteogi.github.io/master/MIT-LICENSE"

Target.create "Pack" (fun _ ->
    let pack project =

        let projectPath = sprintf "core/%s/%s.csproj" project project
        let args =
            let defaultArgs = MSBuild.CliArguments.Create()
            { defaultArgs with
                      Properties = [
                          "Title", project
                          "PackageVersion", nugetVersion
                          "Authors", (String.Join(" ", authors))
                          "Owners", owner
                          "PackageRequireLicenseAcceptance", "false"
                          "Description", description
                          "Summary", summary
                          "Copyright", copyright
                          "PackageTags", tags
                          "PackageProjectUrl", projectUrl
                          "PackageLicenseUrl", licenceUrl
                      ] }

        DotNet.pack (fun p ->
            { p with
                  Configuration = configuration
                  OutputPath = Some "./build"
                  MSBuildParams = args
              }) projectPath

    pack "CodeWriter.Core"
)


// Targets
Target.create "Clean" (fun _ ->
  Shell.cleanDirs [buildDir; testDir]
)

Target.create "Default" (fun _ ->
  Trace.trace "Hello World from FAKE"
)

Target.create "Push" (fun _ ->
    let setNugetPushParams (defaults:NuGet.NuGetPushParams) =
            { defaults with
                Source = Some "https://api.nuget.org/v3/index.json"
                ApiKey = Some nugetApiKye

             }
    let setParams (defaults:DotNet.NuGetPushOptions) =
            { defaults with
                PushParams = setNugetPushParams defaults.PushParams
             }

    IO.Directory.EnumerateFiles(buildDir, "*.nupkg", SearchOption.TopDirectoryOnly)
    |> Seq.iter (fun nupkg ->
        DotNet.nugetPush setParams nupkg
    )
)

open Fake.Core.TargetOperators
"Clean"
  ==> "Pack"
  ==> "Push"
  ==> "Default"

// start build
Target.runOrDefault "Default"
