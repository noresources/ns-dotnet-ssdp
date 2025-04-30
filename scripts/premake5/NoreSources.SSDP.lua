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
		"../../src/NoreSources/**.cs"
	}
	links { 
		"System.dll" ,
		"System.Net.Http.dll"
	} 