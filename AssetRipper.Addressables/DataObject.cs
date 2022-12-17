using System.Text.Json.Serialization;

namespace AssetRipper.Addressables;

public sealed record class DataObject
{
	[JsonPropertyName("m_Id")]
	public string? Id { get; set; }

	[JsonPropertyName("m_ObjectType")]
	public FullyQualifiedName? ObjectType { get; set; }

	[JsonPropertyName("m_Data")]
	public string? Data { get; set; }
}
