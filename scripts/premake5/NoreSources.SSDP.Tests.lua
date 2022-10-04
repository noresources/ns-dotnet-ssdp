--
-- Copyright © 2022 by Renaud Guillard (dev@nore.fr)
-- Distributed under the terms of the MIT License, see LICENSE
--

project "SampleTest"
	kind "ConsoleApp"
	language "C#"
	files { "../../src/tests/SampleTest.cs" }
	links { 
		"NoreSources.SSDP",
		"System.dll",
		"System.Net.Http.dll"
	 }

	