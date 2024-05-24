--
-- Copyright © 2022 by Renaud Guillard (dev@nore.fr)
-- Distributed under the terms of the MIT License, see LICENSE
--

workspace "ns-dotnet-ssdp"
	
	configurations {"Debug", "Release"}
	location (path.join ("..", _ACTION))
	targetdir ("../../bin")
	objdir "../../obj"
	
	include "./NoreSources.Build.lua"
	include "./NoreSources.SSDP.lua"
	include "./NoreSources.SSDP.Examples.lua"