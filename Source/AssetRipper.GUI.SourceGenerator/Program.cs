using Newtonsoft.Json;

namespace AssetRipper.GUI.SourceGenerator;

public static class Program
{
	public static void Main()
	{
		CleanJsonLocalizationFiles();
		LocalizationSourceGenerator.MakeLocalizationClass();
		LocalizationSourceGenerator.MakeLocalizationLoaderClass();
		SettingsPageGenerator.Run();
	}

	private static void CleanJsonLocalizationFiles()
	{
		const string englishPath = Paths.LocalizationsPath + "en_US.json";
		Dictionary<string, string> englishDictionary = DeserializeJson(File.ReadAllText(englishPath))
			.OrderBy(pair => pair.Key)
			.ToDictionary(pair => pair.Key, pair => pair.Value);
		File.WriteAllText(englishPath, SerializeJson(englishDictionary));

		static Dictionary<string, string> DeserializeJson(string jsonText) => DictionarySerializerContext.Deserialize(jsonText);
		static string SerializeJson(Dictionary<string, string> dictionary)
		{
			//We only use Newtonsoft.Json for serialization because it offers control over indentation and newlines.
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
