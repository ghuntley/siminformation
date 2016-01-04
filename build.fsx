#r @"tools/FAKE.Core/tools/FakeLib.dll"
#load "tools/SourceLink.Fake/tools/SourceLink.fsx"
open Fake 
open System
open SourceLink

let authors = ["Geoffrey Huntley"]

// project name and description
let projectName = "SimInformation"
let projectDescription = "SimInformation is a cross-platform library that provides a way to access information on your SIM card."
let projectSummary = projectDescription // TODO: write a summary

// directories
let buildDir = "./SimInformation/bin"
let testResultsDir = "./testresults"
let packagingRoot = "./packaging/"
let samplesDir = "./samples"
let packagingDir = packagingRoot @@ "SimInformation"

let releaseNotes = 
    ReadFile "ReleaseNotes.md"
    |> ReleaseNotesHelper.parseReleaseNotes

let buildMode = getBuildParamOrDefault "buildMode" "Release"

MSBuildDefaults <- { 
    MSBuildDefaults with 
        ToolsVersion = Some "14.0"
        Verbosity = Some MSBuildVerbosity.Minimal }

Target "Clean" (fun _ ->
    CleanDirs [buildDir; testResultsDir; packagingRoot; packagingDir]
)

open Fake.AssemblyInfoFile
open Fake.Testing

Target "AssemblyInfo" (fun _ ->
    CreateCSharpAssemblyInfo "./SolutionInfo.cs"
      [ Attribute.Product projectName
        Attribute.Version releaseNotes.AssemblyVersion
        Attribute.FileVersion releaseNotes.AssemblyVersion
        Attribute.ComVisible false ]
)

Target "CheckProjects" (fun _ ->
    !! "./SimInformation/SimInformation*.csproj"
    |> Fake.MSBuild.ProjectSystem.CompareProjectsTo "./SimInformation/SimInformation.csproj"

    !! "./SimInformation.Reactive/SimInformation.Reactive*.csproj"
    |> Fake.MSBuild.ProjectSystem.CompareProjectsTo "./SimInformation.Reactive/SimInformation.Reactive.csproj"
)


Target "FixProjects" (fun _ ->
    !! "./SimInformation/SimInformation*.csproj"
    |> Fake.MSBuild.ProjectSystem.FixProjectFiles "./SimInformation/SimInformation.csproj"
)

let setParams defaults = {
    defaults with
        ToolsVersion = Some("14.0")
        Targets = ["Build"]
        Properties =
            [
                "Configuration", buildMode
            ]
    }

let Exec command args =
    let result = Shell.Exec(command, args)
    if result <> 0 then failwithf "%s exited with error %d" command result

Target "BuildApp" (fun _ ->
    build setParams "./SimInformation.sln"
        |> DoNothing
)

Target "BuildMono" (fun _ ->
    // xbuild does not support msbuild  tools version 14.0 and that is the reason
    // for using the xbuild command directly instead of using msbuild
    Exec "xbuild" "./SimInformation-Mono.sln /t:Build /tv:12.0 /v:m  /p:RestorePackages='False' /p:Configuration='Release' /logger:Fake.MsBuildLogger+ErrorLogger,'../SimInformation.net/tools/FAKE.Core/tools/FakeLib.dll'"

)

Target "UnitTests" (fun _ ->
    !! (sprintf "./SimInformation.Tests/bin/%s/**/SimInformation.Tests*.dll" buildMode)
    |> xUnit2 (fun p -> 
            {p with
                HtmlOutputPath = Some (testResultsDir @@ "xunit.html") })
)

Target "SourceLink" (fun _ ->
    [   "SimInformation/SimInformation.csproj"
        "SimInformation.UWP/SimInformation.UWP.csproj" ]
    |> Seq.iter (fun pf ->
        let proj = VsProj.LoadRelease pf
        let url = "https://raw.githubusercontent.com/SimInformation/SimInformation.net/{0}/%var2%"
        SourceLink.Index proj.Compiles proj.OutputFilePdb __SOURCE_DIRECTORY__ url
    )
)

Target "CreateSimInformationPackage" (fun _ ->
    let net45Dir = packagingDir @@ "lib/net45/"
    let netcore45Dir = packagingDir @@ "lib/netcore451/"
    let portableDir = packagingDir @@ "lib/portable-net45+wp80+win+wpa81/"
    let linqpadSamplesDir = packagingDir @@ "linqpad-samples"
    CleanDirs [net45Dir; netcore45Dir; portableDir;linqpadSamplesDir]

    CopyFile net45Dir (buildDir @@ "Release/Net45/SimInformation.dll")
    CopyFile net45Dir (buildDir @@ "Release/Net45/SimInformation.XML")
    CopyFile net45Dir (buildDir @@ "Release/Net45/SimInformation.pdb")
    CopyFile netcore45Dir (buildDir @@ "Release/NetCore45/SimInformation.dll")
    CopyFile netcore45Dir (buildDir @@ "Release/NetCore45/SimInformation.XML")
    CopyFile netcore45Dir (buildDir @@ "Release/NetCore45/SimInformation.pdb")
    CopyFile portableDir (buildDir @@ "Release/Portable/SimInformation.dll")
    CopyFile portableDir (buildDir @@ "Release/Portable/SimInformation.XML")
    CopyFile portableDir (buildDir @@ "Release/Portable/SimInformation.pdb")
    CopyDir packagingDir "./samples" allFiles
    CopyFiles packagingDir ["LICENSE.txt"; "README.md"; "ReleaseNotes.md"]

    NuGet (fun p -> 
        {p with
            Authors = authors
            Project = projectName
            Description = projectDescription
            OutputPath = packagingRoot
            Summary = projectSummary
            WorkingDir = packagingDir
            Version = releaseNotes.AssemblyVersion
            ReleaseNotes = toLines releaseNotes.Notes
            AccessKey = getBuildParamOrDefault "nugetkey" ""
            Publish = hasBuildParam "nugetkey" }) "SimInformation.nuspec"
)

Target "Default" DoNothing

Target "CreatePackages" DoNothing


"Clean"
   ==> "AssemblyInfo"
   ==> "CheckProjects"
   ==> "BuildApp"

"Clean"
   ==> "AssemblyInfo"
   ==> "CheckProjects"
   ==> "BuildMono"

"UnitTests"
   ==> "Default"

"ConventionTests"
   ==> "Default"

"IntegrationTests"
   ==> "Default"

"SourceLink"
   ==> "CreatePackages"

"CreateSimInformationPackage"
   ==> "CreatePackages"

RunTargetOrDefault "Default"