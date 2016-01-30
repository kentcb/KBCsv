@echo off
".nuget\nuget.exe" "install" "FAKE" "-Version" "3.30.1" "-OutputDirectory" "Src\packages\tools" "-ExcludeVersion"
"Src\packages\FAKE\tools\Fake.exe" build.fsx %*