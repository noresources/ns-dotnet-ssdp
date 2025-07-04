SOLUTION := scripts/vs2019/ns-dotnet-ssdp.sln

ifndef verbosity
	verbosity = normal
endif

ifndef configuration
	configuration = Release
endif

ifndef framework
	framework = net8.0
endif

dotnet_options = -nologo \
	--configuration $(configuration) \
	--framework $(framework) \
	--verbosity $(verbosity)

.PHONY: all assemblyinfo build build-scripts clean code-format gmake vs

all: assemblyinfo build

build: 
	@dotnet build $(dotnet_options) "$(SOLUTION)"

clean: 
	@dotnet clean $(dotnet_options) "$(SOLUTION)"

assemblyinfo: 
	@echo Generate AssemblyInfo.cs
	@premake5 --file=scripts/premake5.lua assemblyinfo
	@echo Code format file
	@dotnet format src/NoreSources/SSDP/AssemblyInfo

gmake:
	@premake5 --file=scripts/premake5.lua gmake

vs:
	@premake5 --file=scripts/premake5.lua vs2019
	
build-scripts: vs gmake
	@echo 'Visual Studio projects & GNU Makefile generated'
	
code-format:
	@dotnet format "$(SOLUTION)"
	@dotnet format src/NoreSources/SSDP/AssemblyInfo

