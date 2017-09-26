@echo off
SET dopause=0
IF "%1"=="" (
  SET dopause=1
  SET target=Default
) ELSE (
  SET target=%1
)
cls

@IF NOT EXIST "_build_tools\FAKE\tools\Fake.exe" (
	@echo "Installing FAKE from NUGET... (this may take a short while)"
	@"_build_tools\nuget.exe" "install" "FAKE" "-OutputDirectory" "_build_tools" "-ExcludeVersion"
)

@IF NOT EXIST "_build_tools\NUnit.Runners\tools\nunit-console.exe" (
	@echo "Installing NUNIT from NUGET... (this may take a short while)"
	@"_build_tools\nuget.exe" "install" "NUNIT.Runners" "-OutputDirectory" "_build_tools" "-ExcludeVersion" "-Version" "2.6.4"
)

@echo Invoking build.fsx...
@"_build_tools\FAKE\tools\Fake.exe" build_script.fsx %target%
IF "%dopause%"=="1" pause

