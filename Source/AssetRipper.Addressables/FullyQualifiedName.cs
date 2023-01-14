using System.Text.Json.Serialization;

namespace AssetRipper.Addressables;

public sealed record class FullyQualifiedName
{
	[JsonPropertyName("m_AssemblyName")]
	public string? AssemblyName { get; set; }

	[JsonPropertyName("m_ClassName")]
	public string? ClassName { get; set; }
}
