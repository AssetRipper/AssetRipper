using System.IO;

namespace AssetRipper.GUI.SourceGenerator;

public static class Program
{
	public static void Main(string[] args)
	{
		string repositoryPath = "../../../../";
		string outputPath = Path.Combine(repositoryPath, "AssetRipper.GUI", "LocalizationManager.g.cs");
		
		string source = LocalizationSourceGenerator.MakeLocalizationClass(repositoryPath);

		File.WriteAllText(outputPath, source);
	}
}
