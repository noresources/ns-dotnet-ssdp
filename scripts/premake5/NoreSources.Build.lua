--
-- Copyright © 2022 by Renaud Guillard (dev@nore.fr)
-- Distributed under the terms of the MIT License, see LICENSE
--

project "NoreSources.Build"
	kind "SharedLib"
	targetdir "../../build"
	language "C#"
	dotnetframework "4.7"
	files {
		"../../src/build/**.cs"
	}
	links { 
		"System.dll",
		"Microsoft.Build.dll",
		"Microsoft.Build.Utilities.v4.0.dll",
		-- "Microsoft.Build.Utilities.dll",
		"Microsoft.Build.Framework.dll"
	} 