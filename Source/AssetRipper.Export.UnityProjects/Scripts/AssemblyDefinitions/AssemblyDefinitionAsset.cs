using System.Text.Json.Serialization;

namespace AssetRipper.Export.UnityProjects.Scripts.AssemblyDefinitions;

public sealed class AssemblyDefinitionAsset
{
	[JsonPropertyName("name")]
	public string Name { get; set; }
	[JsonPropertyName("references")]
	public List<string> References { get; set; }
	[JsonPropertyName("allowUnsafeCode")]
	public bool AllowUnsafeCode { get; set; }

	public AssemblyDefinitionAsset(string name)
	{
		Name = name;
		AllowUnsafeCode = true;
		References = new();
	}
}
