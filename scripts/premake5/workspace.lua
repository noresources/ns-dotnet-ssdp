--
-- Copyright © 2022 by Renaud Guillard (dev@nore.fr)
-- Distributed under the terms of the MIT License, see LICENSE
--

workspace "ns-dotnet-ssdp"
	
	configurations {"Debug", "Release"}
	location (path.join ("..", _ACTION))
	objdir "../../obj"
	dotnetframework "net9.0"
	filter {"action:vs*"}
		location (path.join ("..", _ACTION, '%{prj and prj.name or "."}'))
		vsprops {
			TargetFrameworks = "net47;net8.0;net9.0"
		}
	filter {}
	
	include "./NoreSources.SSDP.lua"
	include "./NoreSources.SSDP.Examples.lua"