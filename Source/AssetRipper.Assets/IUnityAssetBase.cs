using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Export.Yaml;
using AssetRipper.Assets.IO.Serialization;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Endian;
using System.Text.Json.Nodes;

namespace AssetRipper.Assets;

public interface IUnityAssetBase : IEndianSpanReadable, IAssetWritable, IYamlExportable
{
	void CopyValues(IUnityAssetBase? source, PPtrConverter converter);
	void Deserialize(JsonNode node, IUnityAssetDeserializer deserializer, DeserializationOptions options);
	void Reset();
	/// <summary>
	/// Serialize this asset as a <see cref="JsonNode"/> using standardized naming.
	/// </summary>
	/// <param name="serializer">An optional serializer for overriding serialization.</param>
	/// <returns>A new <see cref="JsonNode"/> containing this asset's serialized data.</returns>
	JsonNode SerializeAllFields(IUnityAssetSerializer serializer, SerializationOptions options);
	/// <summary>
	/// Serialize this asset as a <see cref="JsonNode"/> using original naming.
	/// </summary>
	/// <param name="serializer">An optional serializer for overriding serialization.</param>
	/// <returns>A new <see cref="JsonNode"/> containing this asset's serialized data.</returns>
	JsonNode SerializeEditorFields(IUnityAssetSerializer serializer, SerializationOptions options);
	/// <summary>
	/// Serialize this asset as a <see cref="JsonNode"/> using original naming.
	/// </summary>
	/// <param name="serializer">An optional serializer for overriding serialization.</param>
	/// <returns>A new <see cref="JsonNode"/> containing this asset's serialized data.</returns>
	JsonNode SerializeReleaseFields(IUnityAssetSerializer serializer, SerializationOptions options);
	IEnumerable<(string, PPtr)> FetchDependencies();
}
