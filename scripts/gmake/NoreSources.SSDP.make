# GNU Make project makefile autogenerated by Premake

ifndef config
  config=debug
endif

ifndef verbose
  SILENT = @
endif

.PHONY: clean prebuild

ifeq ($(config),debug)
  CSC = csc
  RESGEN = resgen
TARGETDIR = ../../lib/Debug
TARGET = $(TARGETDIR)/NoreSources.SSDP.dll
OBJDIR = ../../obj/Debug/NoreSources.SSDP
  FLAGS = /noconfig
  DEPENDS =
  REFERENCES = 
define PREBUILDCMDS
endef
define PRELINKCMDS
endef
define POSTBUILDCMDS
endef
endif

ifeq ($(config),release)
  CSC = csc
  RESGEN = resgen
TARGETDIR = ../../lib/Release
TARGET = $(TARGETDIR)/NoreSources.SSDP.dll
OBJDIR = ../../obj/Release/NoreSources.SSDP
  FLAGS = /noconfig
  DEPENDS =
  REFERENCES = 
define PREBUILDCMDS
endef
define PRELINKCMDS
endef
define POSTBUILDCMDS
endef
endif

FLAGS += /t:library 
REFERENCES += /r:System.dll /r:System.Net.Http.dll

SOURCES += \
	../../src/NoreSources/Collections/Utility.cs \
	../../src/NoreSources/HTTP/Messages.cs \
	../../src/NoreSources/HTTP/Utility.cs \
	../../src/NoreSources/SSDP/AssemblyInfo/AssemblyInfo.cs \
	../../src/NoreSources/SSDP/Message.cs \
	../../src/NoreSources/SSDP/Notification.cs \
	../../src/NoreSources/SSDP/Protocol.cs \
	../../src/NoreSources/SSDP/Search.cs \

EMBEDFILES += \

RESPONSE += $(OBJDIR)/NoreSources.SSDP.rsp
SHELLTYPE := posix
ifeq ($(shell echo "test"), "test")
	SHELLTYPE := msdos
endif

all: prebuild $(EMBEDFILES) $(COPYFILES) $(TARGET)

$(TARGET): $(SOURCES) $(EMBEDFILES) $(DEPENDS) $(RESPONSE) | $(TARGETDIR)
	$(PRELINKCMDS)
	$(SILENT) $(CSC) /nologo /out:$@ $(FLAGS) $(REFERENCES) @$(RESPONSE) $(patsubst %,/resource:%,$(EMBEDFILES))
	$(POSTBUILDCMDS)

$(TARGETDIR):
	@echo Creating $(TARGETDIR)
ifeq (posix,$(SHELLTYPE))
	$(SILENT) mkdir -p $(TARGETDIR)
else
	$(SILENT) mkdir $(subst /,\\,$(TARGETDIR))
endif

$(RESPONSE): NoreSources.SSDP.make
	@echo Generating response file
ifeq (posix,$(SHELLTYPE))
	$(SILENT) rm -f $(RESPONSE)
else
	$(SILENT) if exist $(RESPONSE) del $(OBJDIR)\NoreSources.SSDP.rsp
endif
	@echo ../../src/NoreSources/Collections/Utility.cs >> $(RESPONSE)
	@echo ../../src/NoreSources/HTTP/Messages.cs >> $(RESPONSE)
	@echo ../../src/NoreSources/HTTP/Utility.cs >> $(RESPONSE)
	@echo ../../src/NoreSources/SSDP/AssemblyInfo/AssemblyInfo.cs >> $(RESPONSE)
	@echo ../../src/NoreSources/SSDP/Message.cs >> $(RESPONSE)
	@echo ../../src/NoreSources/SSDP/Notification.cs >> $(RESPONSE)
	@echo ../../src/NoreSources/SSDP/Protocol.cs >> $(RESPONSE)
	@echo ../../src/NoreSources/SSDP/Search.cs >> $(RESPONSE)

$(OBJDIR):
	@echo Creating $(OBJDIR)
ifeq (posix,$(SHELLTYPE))
	$(SILENT) mkdir -p $(OBJDIR)
else
	$(SILENT) mkdir $(subst /,\\,$(OBJDIR))
endif

prebuild: | $(OBJDIR)
	$(PREBUILDCMDS)
