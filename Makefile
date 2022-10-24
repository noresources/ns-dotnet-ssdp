cc = msbuild -nologo

.PHONY: all code-format logger nuget-restore

all: logger
	msbuild -nologo -noconsolelogger -logger:build/NoreSources.Build.dll scripts/vs2019/ns-dotnet-ssdp.sln

code-format:
	dotnet format scripts/vs2019/ns-dotnet-ssdp.sln --no-restore 1>/dev/null

logger:
	$(cc) scripts/vs2019/NoreSources.Build.csproj
	
nuget-restore:
	find scripts/vs2019/ -name '*.csproj' -exec nuget restore {} ';'

