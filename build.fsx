#I "Src/packages/FAKE.3.30.1/tools"
#r "FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile
open Fake.EnvironmentHelper
open Fake.MSBuildHelper
open Fake.NuGetHelper

// properties
let semanticVersion = "3.0.3"
let version = (>=>) @"(?<major>\d*)\.(?<minor>\d*)\.(?<build>\d*).*?" "${major}.${minor}.${build}.0" semanticVersion
let configuration = getBuildParamOrDefault "configuration" "Release"
let deployToNuGet = getBuildParamOrDefault "deployToNuGet" "false"
let genDir = "Gen/"
let docDir = "Doc/"
let srcDir = "Src/"
let testDir = genDir @@ "Test"
let nugetDir = genDir @@ "NuGet"
let tempDir = genDir @@ "Temp"

RestorePackages()

Target "Clean" (fun _ ->
    CleanDirs[genDir; testDir; nugetDir; tempDir]

    build (fun p ->
        { p with
            Verbosity = Some(Quiet)
            Targets = ["Clean"]
            Properties = ["Configuration", configuration]
        })
        (srcDir @@ "KBCsv.sln")
)

Target "Build" (fun _ ->
    // generate the shared assembly info
    CreateCSharpAssemblyInfoWithConfig (srcDir @@ "AssemblyInfoCommon.cs")
        [
            Attribute.Version version
            Attribute.FileVersion version
            Attribute.Configuration configuration
            Attribute.Company "Kent Boogaart"
            Attribute.Product "KBCsv"
            Attribute.Copyright "c Copyright. Kent Boogaart."
            Attribute.Trademark ""
            Attribute.Culture ""
            Attribute.CLSCompliant true
            Attribute.StringAttribute("NeutralResourcesLanguage", "en-US", "System.Resources")
            Attribute.StringAttribute("AssemblyInformationalVersion", semanticVersion, "System.Reflection")
        ]
        (AssemblyInfoFileConfig(false))

    build (fun p ->
        { p with
            Verbosity = Some(Quiet)
            Targets = ["Build"]
            Properties =
                [
                    "Optimize", "True"
                    "DebugSymbols", "True"
                    "Configuration", configuration
                ]
        })
        (srcDir @@ "KBCsv.sln")
)

Target "CreateNuGetPackages" (fun _ ->
    // copy binaries
    !! (srcDir @@ "Kent.Boogaart.KBCsv/bin" @@ configuration @@ "Kent.Boogaart.KBCsv.*")
        |> CopyFiles (nugetDir @@ "KBCsv/lib/portable-win+net45+wp8+MonoAndroid10+Xamarin.iOS10+MonoTouch10")
    !! (srcDir @@ "Kent.Boogaart.KBCsv.Extensions/bin" @@ configuration @@ "Kent.Boogaart.KBCsv.Extensions.*")
        |> CopyFiles (nugetDir @@ "KBCsv.Extensions/lib/portable-win+net45+wp8+MonoAndroid10+Xamarin.iOS10+MonoTouch10")
    !! (srcDir @@ "Kent.Boogaart.KBCsv.Extensions.Data/bin" @@ configuration @@ "Kent.Boogaart.KBCsv.Extensions.Data.*")
        |> CopyFiles (nugetDir @@ "KBCsv.Extensions.Data/lib/net45")

    // copy readme
    CreateDir "./Gen/NuGet/KBCsv/"
    CopyFile "./Gen/NuGet/KBCsv/readme.txt" "./Src/readme.txt"

    CreateDir "./Gen/NuGet/KBCsv.Extensions/"
    CopyFile "./Gen/NuGet/KBCsv.Extensions/readme.txt" "./Src/readme.txt"

    CreateDir "./Gen/NuGet/KBCsv.Extensions.Data/"
    CopyFile "./Gen/NuGet/KBCsv.Extensions.Data/readme.txt" "./Src/readme.txt"

    // create the NuGets
    NuGet (fun p ->
        {p with
            Project = "Kent.Boogaart.KBCsv"
            Version = semanticVersion
            OutputPath = nugetDir
            WorkingDir = nugetDir @@ "KBCsv"
            SymbolPackage = NugetSymbolPackage.Nuspec
            Publish = System.Convert.ToBoolean(deployToNuGet)
        })
        (srcDir @@ "KBCsv.nuspec")

    NuGet (fun p ->
        {p with
            Project = "Kent.Boogaart.KBCsv.Extensions"
            Version = semanticVersion
            OutputPath = nugetDir
            WorkingDir = nugetDir @@ "KBCsv.Extensions"
            SymbolPackage = NugetSymbolPackage.Nuspec
            Publish = System.Convert.ToBoolean(deployToNuGet)
        })
        (srcDir @@ "KBCsv.Extensions.nuspec")

    NuGet (fun p ->
        {p with
            Project = "Kent.Boogaart.KBCsv.Extensions.Data"
            Version = semanticVersion
            OutputPath = nugetDir
            WorkingDir = nugetDir @@ "KBCsv.Extensions.Data"
            SymbolPackage = NugetSymbolPackage.Nuspec
            Publish = System.Convert.ToBoolean(deployToNuGet)
        })
        (srcDir @@ "KBCsv.Extensions.Data.nuspec")
)

"Clean"
    ==> "Build"
    ==> "CreateNuGetPackages"

RunTargetOrDefault "CreateNuGetPackages"