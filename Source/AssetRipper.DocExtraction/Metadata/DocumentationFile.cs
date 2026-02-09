using System.Text.Json;

namespace AssetRipper.DocExtraction.MetaData;

public sealed class DocumentationFile
{
	public string UnityVersion { get; set; } = "";
	public List<ClassDocumentation> Classes { get; set; } = new();
	public List<StructDocumentation> Structs { get; set; } = new();
	public List<EnumDocumentation> Enums { get; set; } = new();

	public string ToJson()
	{
		return JsonSerializer.Serialize(this, JsonSourceGenerationContext.Default.DocumentationFile);
	}

	public void SaveAsJson(string path)
	{
		using FileStream fileStream = File.Create(path);
		JsonSerializer.Serialize(fileStream, this, JsonSourceGenerationContext.Default.DocumentationFile);
	}

	public static DocumentationFile FromFile(string path)
	{
		using FileStream fileStream = File.OpenRead(path);
		return JsonSerializer.Deserialize(fileStream, JsonSourceGenerationContext.Default.DocumentationFile)
			?? throw new Exception("Failed to deserialize json");
	}

	public static DocumentationFile FromJson(string text)
	{
		return JsonSerializer.Deserialize(text, JsonSourceGenerationContext.Default.DocumentationFile)
			?? throw new Exception("Failed to deserialize json");
	}
}
