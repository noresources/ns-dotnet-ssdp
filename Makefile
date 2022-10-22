cc = msbuild

.PHONY: all code-format lib nuget-restore

all:
	$(cc) scripts/vs2019/ns-dotnet-ssdp.sln

code-format:
	dotnet format scripts/vs2019/ns-dotnet-ssdp.sln --no-restore 1>/dev/null

lib:
	$(cc) scripts/vs2019/NoreSources.SSDP.csproj
	
nuget-restore:
	find scripts/vs2019/ -name '*.csproj' -exec nuget restore {} ';'

