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

.PHONY: all assemblyinfo build code-format publish

all: assemblyinfo build publish

build: 
	@dotnet build $(dotnet_options) "$(SOLUTION)"

publish: 
	@dotnet publish $(dotnet_options) "$(SOLUTION)"

assemblyinfo: 
	@premake5 --file=scripts/premake5.lua assemblyinfo

code-format:
	@dotnet format "$(SOLUTION)"

