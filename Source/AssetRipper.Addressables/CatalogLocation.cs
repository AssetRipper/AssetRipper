using System.Text.Json.Serialization;

namespace AssetRipper.Addressables;

public sealed record class CatalogLocation
{
	/// <summary>
	/// The collection of keys for this location.
	/// </summary>
	[JsonPropertyName("m_Keys")]
	[FormerlySerializedAs("m_keys")]
	public List<string>? Keys { get; set; }

	/// <summary>
	/// The internal id.
	/// </summary>
	[JsonPropertyName("m_InternalId")]
	[FormerlySerializedAs("m_internalId")]
	public string? InternalId { get; set; }

	/// <summary>
	/// The provider id.
	/// </summary>
	[JsonPropertyName("m_Provider")]
	[FormerlySerializedAs("m_provider")]
	public string? Provider { get; set; }

	/// <summary>
	/// The collection of dependencies for this location.
	/// </summary>
	[JsonPropertyName("m_Dependencies")]
	[FormerlySerializedAs("m_dependencies")]
	public List<string>? Dependencies { get; set; }

	/// <summary>
	/// The type of the resource for the location.
	/// </summary>
	[JsonPropertyName("m_ResourceType")]
	public FullyQualifiedName? ResourceType { get; set; }

	/// <summary>
	/// The optional arbitrary data stored along with location
	/// </summary>
	[JsonPropertyName("SerializedData")]
	public byte[]? SerializedData { get; set; }
}
