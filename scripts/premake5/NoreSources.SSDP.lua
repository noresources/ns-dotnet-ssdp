--
-- Copyright © 2022 by Renaud Guillard (dev@nore.fr)
-- Distributed under the terms of the MIT License, see LICENSE
--

project "NoreSources.SSDP"
	kind "SharedLib"
	targetdir "../../lib/%{cfg.name}"
	objdir "../../obj"
	language "C#"
	files {
		"../../src/NoreSources/HTTP/**.cs",
		"../../src/NoreSources/Collections/**.cs",
		"../../src/NoreSources/SSDP/*.cs"
	}
	
	filter { "action:gmake" }
		files {
			"../../src/NoreSources/SSDP/AssemblyInfo/AssemblyInfo.cs"
		}
	filter {}
	vsprops (include ("./NoreSources.SSDP.AssemblyInfo.lua"))
	
	links { 
		"System.dll" ,
		"System.Net.Http.dll"
	} 