workspace "ns-dotnet-ssdp"
	
	configurations {"Debug", "Release"}
	location (_ACTION)
	targetdir ("../bin")
	
	project "NoeeSources.SSDP"
		kind "SharedLib"
		language "C#"
		dotnetframework "4.7"
		files {
			"../src/%{prj.name}/**.cs"
		}
		links { 
			"System.dll" ,
			"System.Net.Http.dll"
		}	
		
		project "SampleTest"
			kind "ConsoleApp"
			language "C#"
			files { "../src/tests/SampleTest.cs" }
			links { 
				"NoeeSources.SSDP",
				"System.Net.Http.dll"
			 }
		
		