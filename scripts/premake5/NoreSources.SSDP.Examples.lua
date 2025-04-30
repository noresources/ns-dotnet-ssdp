--
-- Copyright © 2022 by Renaud Guillard (dev@nore.fr)
-- Distributed under the terms of the MIT License, see LICENSE
--

for _,filepath in ipairs(os.matchfiles("../../src/examples/*.cs"))
do
	project (path.getbasename(filepath))
		kind "ConsoleApp"
		targetdir "../../bin/%{cfg.name}"
		objdir "../../obj"
		language "C#"
		files (filepath)
		links { 
			"NoreSources.SSDP",
			"System.dll",
			"System.Net.Http.dll"
		 }
		 
		 filter { "action:gmake2" }
			 	postbuildcommands {
			 		"{copyfile} %{cfg.linktarget.directory}/../lib/NoreSources.SSDP.dll %{cfg.linktarget.directory}"
			 	}
		 filter {}
end