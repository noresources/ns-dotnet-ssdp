cc = dotnet build -nologo
verbosity = normal

.PHONY: all code-format

all: 
	@$(MAKE) -C scripts/gmake2

code-format:
	@dotnet format scripts/vs2019/ns-dotnet-ssdp.sln --no-restore 1>/dev/null

