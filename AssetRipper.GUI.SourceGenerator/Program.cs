using System.IO;

namespace AssetRipper.GUI.SourceGenerator;

public static class Program
{
	private const string repositoryPath = "../../../../";

	private static string GetPathForProject(string project)
	{
		return Path.Combine(repositoryPath, project);
	}

	public static void Main(string[] args)
	{
		string localizationPath = Path.Combine(GetPathForProject("AssetRipper.GUI"), "LocalizationManager.g.cs");
		File.WriteAllText(localizationPath, LocalizationSourceGenerator.MakeLocalizationClass(repositoryPath));
	}
}
