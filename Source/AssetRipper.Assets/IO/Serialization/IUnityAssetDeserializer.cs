using AssetRipper.Assets.Metadata;
using System.Text.Json.Nodes;

namespace AssetRipper.Assets.IO.Serialization;

/// <summary>
/// An interface for doing custom deserialization of <see cref="IUnityAssetBase"/>s.
/// </summary>
public interface IUnityAssetDeserializer
{
	/// <summary>
	/// Try deserialize <paramref name="node"/> to <paramref name="asset"/>.
	/// </summary>
	/// <typeparam name="T">The type of asset being deserialized.</typeparam>
	/// <param name="asset">The asset being deserialized.</param>
	/// <param name="node">A <see cref="JsonNode"/> containing the serialized data.</param>
	/// <returns>True if successfully deserialized.</returns>
	bool TryDeserialize<T>(T asset, JsonNode node, DeserializationOptions options)
		where T : IUnityAssetBase;

	/// <summary>
	/// Try deserialize <paramref name="node"/> to <paramref name="pptr"/>.
	/// </summary>
	/// <typeparam name="TPPtr">The type of PPtr being deserialized.</typeparam>
	/// <typeparam name="TAsset">The type of asset referenced by <typeparamref name="TPPtr"/>.</typeparam>
	/// <param name="pptr">The PPtr being deserialized.</param>
	/// <param name="node">A <see cref="JsonNode"/> containing the serialized data.</param>
	/// <returns>True if successfully deserialized.</returns>
	bool TryDeserialize<TPPtr, TAsset>(TPPtr pptr, JsonNode node, DeserializationOptions options)
		where TPPtr : IPPtr<TAsset>
		where TAsset : IUnityObjectBase;
}
