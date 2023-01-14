using System.Text.Json.Serialization;

namespace AssetRipper.Addressables;

public sealed record class Settings
{
	[JsonPropertyName("m_buildTarget")]
	public string? BuildTarget { get; set; }

	[JsonPropertyName("m_SettingsHash")]
	public string? SettingsHash { get; set; }

	[JsonPropertyName("m_CatalogLocations")]
	public List<CatalogLocation>? CatalogLocations { get; set; }

	[JsonPropertyName("m_ProfileEvents")]
	public bool ProfileEvents { get; set; }

	[JsonPropertyName("m_LogResourceManagerExceptions")]
	public bool LogResourceManagerExceptions { get; set; }

	[JsonPropertyName("m_ExtraInitializationData")]
	public byte[]? ExtraInitializationData { get; set; }//might not be byte[]

	[JsonPropertyName("m_DisableCatalogUpdateOnStart")]
	public bool DisableCatalogUpdateOnStart { get; set; }

	[JsonPropertyName("m_IsLocalCatalogInBundle")]
	public bool IsLocalCatalogInBundle { get; set; }

	[JsonPropertyName("m_CertificateHandlerType")]
	public FullyQualifiedName? CertificateHandlerType { get; set; }

	[JsonPropertyName("m_AddressablesVersion")]
	public string? AddressablesVersion { get; set; }

	[JsonPropertyName("m_maxConcurrentWebRequests")]
	public int MaxConcurrentWebRequests { get; set; }

	[JsonPropertyName("m_CatalogRequestsTimeout")]
	public int CatalogRequestsTimeout { get; set; }
}
