using System.IO;

namespace AssetRipper.GUI.SourceGenerator;

public static class Program
{
	const string RepositoryPath = "../../../../";
	const string GuiProjectPath = RepositoryPath + "AssetRipper.GUI/";

	public static void Main()
	{
		GenerateLocalizationManagerFile();
	}

	private static void GenerateLocalizationManagerFile()
	{
		const string outputPath = GuiProjectPath + "LocalizationManager.g.cs";
		string source = LocalizationSourceGenerator.MakeLocalizationClass(RepositoryPath);
		File.WriteAllText(outputPath, source);
	}
}
