using System;

// Parameters.
var projectName = "KBCsv";
var semanticVersion = "5.0.0";
var version = semanticVersion + ".0";
var configuration = EnvironmentVariable("CONFIGURATION") ?? "Release";
var nugetSource = "https://www.nuget.org/api/v2/package";

// To push to NuGet, run with: & {$env:PUSH="true"; ./build.ps1}
var push = bool.Parse(EnvironmentVariable("PUSH") ?? "false");

// Paths.
var srcDir = Directory("Src");
var solution = srcDir + File(projectName + ".sln");

Setup(context => Information("Building {0} version {1}.", projectName, version));

Teardown(context => Information("Build {0} finished.", version));

Task("Clean")
    .Does(() => DotNetCoreClean(solution));

Task("Pre-Build")
    .Does(
        () =>
        {
            CreateAssemblyInfo(
                srcDir + File("AssemblyInfoCommon.cs"),
                new AssemblyInfoSettings
                {
                    Company = "Kent Boogaart",
                    Product = projectName,
                    Copyright = "© Copyright. Kent Boogaart.",
                    Version = version,
                    FileVersion = version,
                    InformationalVersion = semanticVersion.ToString(),
                    Configuration = configuration
                }
                .AddCustomAttribute("NeutralResourcesLanguage", "System.Resources", "en-US"));
        });

Task("Build")
    .IsDependentOn("Pre-Build")
    .Does(
        () =>
        {
            var msBuildSettings = new DotNetCoreMSBuildSettings();
            msBuildSettings.Properties["Version"] = new string[]
            {
                version,
            };

            var settings = new DotNetCoreBuildSettings
            {
                Configuration = configuration,
                MSBuildSettings = msBuildSettings,
            };

            DotNetCoreBuild(solution, settings);
        });

Task("Test")
    .IsDependentOn("Build")
    .Does(
        () =>
        {
            var testProjects = new []
            {
                srcDir + Directory("KBCsv.UnitTests") + File("KBCsv.UnitTests.csproj"),
                srcDir + Directory("KBCsv.Extensions.UnitTests") + File("KBCsv.Extensions.UnitTests.csproj"),
                srcDir + Directory("KBCsv.Extensions.Data.UnitTests") + File("KBCsv.Extensions.Data.UnitTests.csproj"),
            };

            foreach (var testProject in testProjects)
            {
                DotNetCoreTest(testProject);
            }
        });

Task("Push")
    .IsDependentOn("Test")
    .WithCriteria(push)
    .Does(
        () =>
        {
            var settings = new DotNetCoreNuGetPushSettings
            {
                Source = nugetSource,
            };

            var nuGetPackages = new []
            {
                srcDir + Directory("KBCsv/bin") + Directory(configuration) + File("KBCsv." + semanticVersion + ".nupkg"),
                srcDir + Directory("KBCsv.Extensions/bin") + Directory(configuration) + File("KBCsv.Extensions." + semanticVersion + ".nupkg"),
                srcDir + Directory("KBCsv.Extensions.Data/bin") + Directory(configuration) + File("KBCsv.Extensions.Data." + semanticVersion + ".nupkg"),
            };

            foreach (var nuGetPackage in nuGetPackages)
            {
                DotNetCoreNuGetPush(nuGetPackage, settings);
            }
        });

Task("Default")
    .IsDependentOn("Push");

RunTarget(Argument("target", "Default"));