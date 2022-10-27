cc = msbuild -nologo
verbosity = normal

.PHONY: all code-format logger nuget-restore

all: build/NoreSources.Build.dll
	@msbuild -m -nologo -noconsolelogger -verbosity:$(verbosity) -logger:build/NoreSources.Build.dll scripts/vs2019/ns-dotnet-ssdp.sln

code-format:
	@dotnet format scripts/vs2019/ns-dotnet-ssdp.sln --no-restore 1>/dev/null

build/NoreSources.Build.dll: src/build/Logger.cs
	@$(cc) scripts/vs2019/NoreSources.Build.csproj
	
logger: build/NoreSources.Build.dll
	@echo -n ''	
	
nuget-restore:
	@find scripts/vs2019/ -name '*.csproj' -exec nuget restore {} ';'

