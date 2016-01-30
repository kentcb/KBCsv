@echo off
cls
".nuget\NuGet.exe" "Install" "FAKE" "-OutputDirectory" "Src\packages" "-ExcludeVersion"
"Src\packages\FAKE\tools\Fake.exe" build.fsx %*