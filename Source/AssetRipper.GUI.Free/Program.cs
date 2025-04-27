using AssetRipper.GUI.Web;
using AssetRipper.Import.Utils;
using AssetRipper.SourceGenerated.NativeEnums.Global;

internal static class Program
{
	//[STAThread]
	//private static void Main(string[] args) => WebApplicationLauncher.Launch(args);
	[STAThread]
	private static int Main(string[] args)
	{
		Console.WriteLine("### AssetRipper CLI ###");

		Dictionary<string, string> arguments = ParseArguments(args);

		//ImportSettings
		if (arguments.ContainsKey("ScriptContentLevel"))
		{
			Console.WriteLine("Setting ScriptContentLevel to " + arguments["ScriptContentLevel"]);
			GameFileLoader.Settings.ImportSettings.ScriptContentLevel = (AssetRipper.Import.Configuration.ScriptContentLevel)int.Parse(arguments["ScriptContentLevel"]);
			Console.WriteLine(GameFileLoader.Settings.ImportSettings.ScriptContentLevel);
		}
		//"StreamingAssetsMode": 1,
		if (arguments.ContainsKey("StreamingAssetsMode"))
		{
			Console.WriteLine("Setting StreamingAssetsMode to " + arguments["StreamingAssetsMode"]);
			GameFileLoader.Settings.ImportSettings.StreamingAssetsMode = (AssetRipper.Import.Configuration.StreamingAssetsMode)int.Parse(arguments["StreamingAssetsMode"]);
			Console.WriteLine(GameFileLoader.Settings.ImportSettings.StreamingAssetsMode);
		}
		//ProcessingSettings "EnablePrefabOutlining": false,
		if (arguments.ContainsKey("EnablePrefabOutlining"))
		{
			Console.WriteLine("Setting EnablePrefabOutlining to " + arguments["EnablePrefabOutlining"]);
			GameFileLoader.Settings.ProcessingSettings.EnablePrefabOutlining = bool.Parse(arguments["EnablePrefabOutlining"]);
			Console.WriteLine(GameFileLoader.Settings.ProcessingSettings.EnablePrefabOutlining);
		}
		//"EnableStaticMeshSeparation": false,
		if (arguments.ContainsKey("EnableStaticMeshSeparation"))
		{
			Console.WriteLine("Setting EnableStaticMeshSeparation to " + arguments["EnableStaticMeshSeparation"]);
			GameFileLoader.Settings.ProcessingSettings.EnableStaticMeshSeparation = bool.Parse(arguments["EnableStaticMeshSeparation"]);
			Console.WriteLine(GameFileLoader.Settings.ProcessingSettings.EnableStaticMeshSeparation);
		}
		//"EnableAssetDeduplication": false,
		if (arguments.ContainsKey("EnableAssetDeduplication"))
		{
			Console.WriteLine("Setting EnableAssetDeduplication to " + arguments["EnableAssetDeduplication"]);
			GameFileLoader.Settings.ProcessingSettings.EnableAssetDeduplication = bool.Parse(arguments["EnableAssetDeduplication"]);
			Console.WriteLine(GameFileLoader.Settings.ProcessingSettings.EnableAssetDeduplication);
		}
		//"RemoveNullableAttributes": false,
		if (arguments.ContainsKey("RemoveNullableAttributes"))
		{
			Console.WriteLine("Setting RemoveNullableAttributes to " + arguments["RemoveNullableAttributes"]);
			GameFileLoader.Settings.ProcessingSettings.RemoveNullableAttributes = bool.Parse(arguments["RemoveNullableAttributes"]);
			Console.WriteLine(GameFileLoader.Settings.ProcessingSettings.RemoveNullableAttributes);
		}
		//"BundledAssetsExportMode": 2
		if (arguments.ContainsKey("BundledAssetsExportMode"))
		{
			Console.WriteLine("Setting BundledAssetsExportMode to " + arguments["BundledAssetsExportMode"]);
			GameFileLoader.Settings.ProcessingSettings.BundledAssetsExportMode = (AssetRipper.Processing.Configuration.BundledAssetsExportMode)int.Parse(arguments["BundledAssetsExportMode"]);
			Console.WriteLine(GameFileLoader.Settings.ProcessingSettings.BundledAssetsExportMode);
		}
		//ExportSettings
		/*
		"AudioExportFormat": 2,
    "ImageExportFormat": 4,
    "LightmapTextureExportFormat": 2,
    "ScriptExportMode": 1,
    "ScriptLanguageVersion": -1,
    "ScriptTypesFullyQualified": false,
    "ShaderExportMode": 3,
    "SpriteExportMode": 0,
    "TextExportMode": 2,
    "SaveSettingsToDisk": true,
    "LanguageCode": null*/
		if (arguments.ContainsKey("AudioExportFormat"))
		{
			Console.WriteLine("Setting AudioExportFormat to " + arguments["AudioExportFormat"]);
			GameFileLoader.Settings.ExportSettings.AudioExportFormat = (AssetRipper.Export.UnityProjects.Configuration.AudioExportFormat)int.Parse(arguments["AudioExportFormat"]);
			Console.WriteLine(GameFileLoader.Settings.ExportSettings.AudioExportFormat);
		}
		if (arguments.ContainsKey("ImageExportFormat"))
		{
			Console.WriteLine("Setting ImageExportFormat to " + arguments["ImageExportFormat"]);
			GameFileLoader.Settings.ExportSettings.ImageExportFormat = (AssetRipper.Export.Modules.Textures.ImageExportFormat)int.Parse(arguments["ImageExportFormat"]);
			Console.WriteLine(GameFileLoader.Settings.ExportSettings.ImageExportFormat);
		}
		if (arguments.ContainsKey("LightmapTextureExportFormat"))
		{
			Console.WriteLine("Setting LightmapTextureExportFormat to " + arguments["LightmapTextureExportFormat"]);
			GameFileLoader.Settings.ExportSettings.LightmapTextureExportFormat = (AssetRipper.Export.UnityProjects.Configuration.LightmapTextureExportFormat)int.Parse(arguments["LightmapTextureExportFormat"]);
			Console.WriteLine(GameFileLoader.Settings.ExportSettings.LightmapTextureExportFormat);
		}
		if (arguments.ContainsKey("ScriptExportMode"))
		{
			Console.WriteLine("Setting ScriptExportMode to " + arguments["ScriptExportMode"]);
			GameFileLoader.Settings.ExportSettings.ScriptExportMode = (AssetRipper.Export.UnityProjects.Configuration.ScriptExportMode)int.Parse(arguments["ScriptExportMode"]);
			Console.WriteLine(GameFileLoader.Settings.ExportSettings.ScriptExportMode);
		}
		if (arguments.ContainsKey("ScriptLanguageVersion"))
		{
			Console.WriteLine("Setting ScriptLanguageVersion to " + arguments["ScriptLanguageVersion"]);
			GameFileLoader.Settings.ExportSettings.ScriptLanguageVersion = (AssetRipper.Export.UnityProjects.Configuration.ScriptLanguageVersion)int.Parse(arguments["ScriptLanguageVersion"]);
			Console.WriteLine(GameFileLoader.Settings.ExportSettings.ScriptLanguageVersion);
		}
		if (arguments.ContainsKey("ScriptTypesFullyQualified"))
		{
			Console.WriteLine("Setting ScriptTypesFullyQualified to " + arguments["ScriptTypesFullyQualified"]);
			GameFileLoader.Settings.ExportSettings.ScriptTypesFullyQualified = bool.Parse(arguments["ScriptTypesFullyQualified"]);
			Console.WriteLine(GameFileLoader.Settings.ExportSettings.ScriptTypesFullyQualified);
		}
		if (arguments.ContainsKey("ShaderExportMode"))
		{
			Console.WriteLine("Setting ShaderExportMode to " + arguments["ShaderExportMode"]);
			GameFileLoader.Settings.ExportSettings.ShaderExportMode = (AssetRipper.Export.UnityProjects.Configuration.ShaderExportMode)int.Parse(arguments["ShaderExportMode"]);
			Console.WriteLine(GameFileLoader.Settings.ExportSettings.ShaderExportMode);
		}
		if (arguments.ContainsKey("SpriteExportMode"))
		{
			Console.WriteLine("Setting SpriteExportMode to " + arguments["SpriteExportMode"]);
			GameFileLoader.Settings.ExportSettings.SpriteExportMode = (AssetRipper.Export.UnityProjects.Configuration.SpriteExportMode)int.Parse(arguments["SpriteExportMode"]);
			Console.WriteLine(GameFileLoader.Settings.ExportSettings.SpriteExportMode);
		}
		if (arguments.ContainsKey("TextExportMode"))
		{
			Console.WriteLine("Setting TextExportMode to " + arguments["TextExportMode"]);
			GameFileLoader.Settings.ExportSettings.TextExportMode = (AssetRipper.Export.UnityProjects.Configuration.TextExportMode)int.Parse(arguments["TextExportMode"]);
			Console.WriteLine(GameFileLoader.Settings.ExportSettings.TextExportMode);
		}

		Console.WriteLine("Set Settings!");

		// Check if the arguments contain the input and output paths
		if (!arguments.ContainsKey("InputPath") || !arguments.ContainsKey("OutputPath"))
		{
			Console.WriteLine("Please provide the input and output paths.");
			return 1;
		}

		string InputPath = arguments["InputPath"];
		string OutputPath = arguments["OutputPath"];


		if(!IsValidPath(InputPath))
		{
			Console.WriteLine("The provided path to the game files is invalid. Please provide a valid path.");
			return 1;
		}
		if (!IsValidPath(OutputPath))
		{
			Console.WriteLine("The provided path to export to is invalid. Please provide a valid path.");
			return 1;
		}
		if (!Directory.Exists(InputPath))
		{
			Console.WriteLine("The provided path to the game files does not exist. Please provide a valid path.");
			return 1;
		}
		/*if (!File.Exists(ExecutingDirectory.Combine("AssetRipper.Settings.json")))
		{
			Console.WriteLine("The AssetRipper.Settings.json file does not exist.");
			return 1;
		}*/
		
		

		Console.WriteLine("Loading game files...");
		GameFileLoader.LoadAndProcess(new List<string> { InputPath });
		if(GameFileLoader.IsLoaded)
		{
			Console.WriteLine("Game file loaded successfully and ready to export");
			Console.WriteLine(GameFileLoader.Settings.ImportSettings.ScriptContentLevel);
			Console.WriteLine("Exporting Unity project...");
			GameFileLoader.ExportUnityProject(OutputPath);
			Console.WriteLine("Exported Unity project successfully to " + OutputPath);
			return 0;
		}
		else
		{
			Console.WriteLine("Failed to load game file from " + InputPath);
			return 1;
		}
	}

	static bool IsValidPath(string path)
	{
		// Check if the string contains any invalid characters
		char[] invalidChars = Path.GetInvalidPathChars();
		if (path.IndexOfAny(invalidChars) >= 0)
		{
			return false; // Contains invalid characters
		}
		// Optional: You could add additional checks like whether the path exists
		return true;
	}

	// Simple argument parsing function
	static Dictionary<string, string> ParseArguments(string[] args)
	{
		var arguments = new Dictionary<string, string>();

		for (int i = 0; i < args.Length; i++)
		{
			// Handle flags (e.g., -verbose)
			if (args[i].StartsWith("-"))
			{
				// Check if there's a value after the flag
				if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
				{
					arguments[args[i].TrimStart('-')] = args[i + 1]; // Save the flag with its value
					i++; // Skip the next argument since it's the value
				}
				else
				{
					arguments[args[i].TrimStart('-')] = null; // It's a flag without a value
				}
			}
		}

		return arguments;
	}
}
