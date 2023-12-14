using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Export.Yaml;
using AssetRipper.Assets.IO.Serialization;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles;
using System.Text.Json.Nodes;

namespace AssetRipper.Assets;

public interface IUnityAssetBase : IEndianSpanReadable, IAssetWritable, IYamlExportable
{
	int SerializedVersion { get; }
	bool FlowMappedInYaml { get; }
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
	/// <summary>
	/// Walk this asset using original naming.
	/// </summary>
	/// <param name="walker">A walker for traversal.</param>
	void WalkEditor(AssetWalker walker);
	/// <summary>
	/// Walk this asset using original naming.
	/// </summary>
	/// <param name="walker">A walker for traversal.</param>
	void WalkRelease(AssetWalker walker);
	/// <summary>
	/// Walk this asset using standardized naming.
	/// </summary>
	/// <param name="walker">A walker for traversal.</param>
	void WalkStandard(AssetWalker walker);
	IEnumerable<(string, PPtr)> FetchDependencies();
}
public static class UnityAssetBaseExtensions
{
	public static void Read(this IUnityAssetBase asset, ref EndianSpanReader reader, TransferInstructionFlags flags)
	{
		if (flags.IsRelease())
		{
			asset.ReadRelease(ref reader);
		}
		else
		{
			asset.ReadEditor(ref reader);
		}
	}
}
