namespace AssetRipper.GUI.SourceGenerator;

public static class Program
{
	public static void Main()
	{
		CleanJsonLocalizationFiles();
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
		static string SerializeJson(Dictionary<string, string> dictionary) => DictionarySerializerContext.Serialize(dictionary);
	}
}
