local ACTION_DIR = _SCRIPT_DIR
local HEADER_FILENAME = path.join(ACTION_DIR, 
			"../../..", 
			"resources/templates/AssemblyInfo.cs.header")
		
local DATA_FILENAME = path.join(ACTION_DIR, 
			"../../..", 
			"resources/data/AssemblyInfo.lua")
local DATA = include (DATA_FILENAME)
		
local TARGET_FILENAME = path.join(ACTION_DIR, 
		"../../..", 
		"src/NoreSources/SSDP/AssemblyInfo/AssemblyInfo.cs")
 



newaction {
	trigger = "assemblyinfo",
	description = "Generate AssemblyInfo.cs from lua data",
	onStart = function ()
	end,
	execute = function ()
		local content = io.readfile (HEADER_FILENAME)
		
		for k, v in pairs (DATA or {})
		do
			content = content .. "\n" 
				.. "[assembly: " .. k 
				.. "(\"" .. premake.esc(v) .. "\")]"
		end
		content = content .. "\n"
		os.writefile_ifnotequal (content, TARGET_FILENAME)
	end
}
