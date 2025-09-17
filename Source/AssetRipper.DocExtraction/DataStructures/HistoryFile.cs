using System.Text.Json;

namespace AssetRipper.DocExtraction.DataStructures;

public sealed class HistoryFile
{
	public Dictionary<string, ClassHistory> Classes { get; set; } = new();
	public Dictionary<string, StructHistory> Structs { get; set; } = new();
	public Dictionary<string, EnumHistory> Enums { get; set; } = new();

	public string ToJson()
	{
		return JsonSerializer.Serialize(this, JsonSourceGenerationContext.Default.HistoryFile);
	}

	public void SaveAsJson(string path)
	{
		using FileStream fileStream = File.Create(path);
		JsonSerializer.Serialize(fileStream, this, JsonSourceGenerationContext.Default.HistoryFile);
	}

	public static HistoryFile FromFile(string path)
	{
		using FileStream fileStream = File.OpenRead(path);
		return JsonSerializer.Deserialize(fileStream, JsonSourceGenerationContext.Default.HistoryFile)
			?? throw new Exception("Failed to deserialize json");
	}

	public static HistoryFile FromJson(string text)
	{
		return JsonSerializer.Deserialize(text, JsonSourceGenerationContext.Default.HistoryFile)
			?? throw new Exception("Failed to deserialize json");
	}
}
