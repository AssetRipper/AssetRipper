using Newtonsoft.Json;

namespace AssetRipper.GUI.SourceGenerator;

public static class Program
{
	const string RepositoryPath = "../../../../";
	const string LocalizationsPath = RepositoryPath + "Localizations/";
	const string GuiProjectPath = RepositoryPath + "AssetRipper.GUI/";

	public static void Main()
	{
		GenerateLocalizationManagerFile();
		//CleanJsonLocalizationFiles();
	}

	private static void GenerateLocalizationManagerFile()
	{
		const string outputPath = GuiProjectPath + "LocalizationManager.g.cs";
		string source = LocalizationSourceGenerator.MakeLocalizationClass(RepositoryPath);
		File.WriteAllText(outputPath, source);
	}

	private static void CleanJsonLocalizationFiles()
	{
		string englishJson = File.ReadAllText(LocalizationsPath + "en_US.json");
		File.WriteAllText(LocalizationsPath + "en_GB.json", englishJson);
		Dictionary<string, string> englishDictionary = DeserializeJson(englishJson);
		foreach (string path in Directory.EnumerateFiles(LocalizationsPath, "*.json"))
		{
			string fileName = Path.GetFileName(path);
			switch (fileName)
			{
				case "en_US.json":
					break;
				case "en_GB.json":
					break;
				default:
					{
						Dictionary<string, string> oldForeignDictionary = DeserializeJson(File.ReadAllText(path));
						Dictionary<string, string> newForeignDictionary = new();
						foreach (string key in englishDictionary.Keys.Order())
						{
							if (oldForeignDictionary.TryGetValue(key, out string? value))
							{
								newForeignDictionary.Add(key, value);
							}
							else
							{
								newForeignDictionary.Add(key, "");
							}
						}
						File.WriteAllText(path, SerializeJson(newForeignDictionary));
					}
					break;
			}
		}

		static Dictionary<string, string> DeserializeJson(string jsonText)
		{
			return JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonText) ?? throw new("Json text could not be deserialized.");
		}
		static string SerializeJson(Dictionary<string, string> dictionary)
		{
			StringWriter stringWriter = new();
			stringWriter.NewLine = "\n";
			JsonTextWriter jsonWriter = new JsonTextWriter(stringWriter)
			{
				Indentation = 4,
				Formatting = Formatting.Indented,
			};
			JsonSerializer.CreateDefault().Serialize(jsonWriter, dictionary);
			jsonWriter.Flush();
			stringWriter.WriteLine();
			return stringWriter.ToString();
		}
	}
}
