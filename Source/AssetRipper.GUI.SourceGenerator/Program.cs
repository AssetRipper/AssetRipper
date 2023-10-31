using Newtonsoft.Json;

namespace AssetRipper.GUI.SourceGenerator;

public static class Program
{
	public static void Main()
	{
		CleanJsonLocalizationFiles();
		LocalizationSourceGenerator.MakeLocalizationClass();
		LocalizationSourceGenerator.MakeLocalizationLoaderClass();
	}

	private static void CleanJsonLocalizationFiles()
	{
		string englishJson = File.ReadAllText(Paths.LocalizationsPath + "en_US.json");
		Dictionary<string, string> englishDictionary = DeserializeJson(englishJson).OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
		foreach (string path in Directory.EnumerateFiles(Paths.LocalizationsPath, "*.json"))
		{
			string fileName = Path.GetFileName(path);
			switch (fileName)
			{
				case "en_US.json":
					File.WriteAllText(path, SerializeJson(englishDictionary));
					break;
				default:
					{
						Dictionary<string, string> oldForeignDictionary = DeserializeJson(File.ReadAllText(path));
						Dictionary<string, string> newForeignDictionary = new(englishDictionary.Count);
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
