@echo off

pushd %~dp0

@rem src\.nuget\NuGet.exe update -self

src\.nuget\NuGet.exe install FAKE -OutputDirectory packages -ExcludeVersion 

src\.nuget\NuGet.exe install xunit.runner.console -OutputDirectory packages\FAKE -ExcludeVersion 

if not exist src\packages\SourceLink.Fake\tools\SourceLink.fsx (
  src\.nuget\nuget.exe install SourceLink.Fake -OutputDirectory packages -ExcludeVersion
)
rem cls

set encoding=utf-8
packages\FAKE\tools\FAKE.exe build.fsx %*

popd
