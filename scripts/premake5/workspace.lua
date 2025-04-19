--
-- Copyright © 2022 by Renaud Guillard (dev@nore.fr)
-- Distributed under the terms of the MIT License, see LICENSE
--

workspace "ns-dotnet-ssdp"
	
	configurations {"Debug", "Release"}
	location (path.join ("..", _ACTION))
	targetdir ("../../bin")
	objdir "../../obj"
	dotnetframework "net8.0"
	vsprops {
		TargetFrameworks = "net47;net8.0"
	}
	
	include "./NoreSources.Build.lua"
	include "./NoreSources.SSDP.lua"
	include "./NoreSources.SSDP.Examples.lua"