--
-- Copyright © 2022 by Renaud Guillard (dev@nore.fr)
-- Distributed under the terms of the MIT License, see LICENSE
--

project "NoreSources.SSDP"
	kind "SharedLib"
	targetdir "../../lib"
	language "C#"
	dotnetframework "4.7"
	files {
		"../../src/NoreSources/**.cs"
	}
	links { 
		"System.dll" ,
		"System.Net.Http.dll"
	} 