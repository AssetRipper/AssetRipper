using System.Text.Json;
using System.Text.Json.Serialization;

namespace AssetRipper.Processing.Addressables;

public class AddressablesCatalog
{
	[JsonPropertyName("m_InternalIds")]
	public string[]? InternalIds { get; set; }

	[JsonPropertyName("m_KeyDataString")]
	public string? KeyDataString { get; set; }

	[JsonPropertyName("m_BucketDataString")]
	public string? BucketDataString { get; set; }

	[JsonPropertyName("m_EntryDataString")]
	public string? EntryDataString { get; set; }

	[JsonPropertyName("m_ExtraDataString")]
	public string? ExtraDataString { get; set; }

	[JsonPropertyName("m_ResourceTypes")]
	public string[]? ResourceTypes { get; set; }
}

public static class AddressablesCatalogParser
{
	public static AddressablesCatalog? ParseJson(string json)
	{
		try
		{
			return JsonSerializer.Deserialize<AddressablesCatalog>(json);
		}
		catch
		{
			return null;
		}
	}
}
