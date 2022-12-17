using System.Text.Json.Serialization;

namespace AssetRipper.Addressables;

public sealed record class CatalogLocation
{
	[JsonPropertyName("m_Keys")]
	public List<string>? Keys { get; set; }

	[JsonPropertyName("m_InternalId")]
	public string? InternalId { get; set; }

	[JsonPropertyName("m_Provider")]
	public string? Provider { get; set; }

	[JsonPropertyName("m_Dependencies")]
	public List<string>? Dependencies { get; set; }

	[JsonPropertyName("m_ResourceType")]
	public FullyQualifiedName? ResourceType { get; set; }

	[JsonPropertyName("SerializedData")]
	public byte[]? SerializedData { get; set; }
}
