using AssetRipper.Assets.Metadata;
using System.Text.Json.Nodes;

namespace AssetRipper.Assets.IO.Serialization;

/// <summary>
/// An interface for doing custom serialization of <see cref="IUnityAssetBase"/>s.
/// </summary>
public interface IUnityAssetSerializer
{
	/// <summary>
	/// Try serialize <paramref name="asset"/> to <paramref name="node"/>.
	/// </summary>
	/// <typeparam name="T">The type of asset being serialized.</typeparam>
	/// <param name="asset">The asset being serialized.</param>
	/// <param name="node">A <see cref="JsonNode"/> containing the serialized data.</param>
	/// <returns>True if successfully serialized.</returns>
	bool TrySerialize<T>(T asset, SerializationOptions options, [NotNullWhen(true)] out JsonNode? node)
		where T : IUnityAssetBase;

	/// <summary>
	/// Try serialize <paramref name="pptr"/> to <paramref name="node"/>.
	/// </summary>
	/// <typeparam name="TPPtr">The type of PPtr being serialized.</typeparam>
	/// <typeparam name="TAsset">The type of asset referenced by <typeparamref name="TPPtr"/>.</typeparam>
	/// <param name="pptr">The PPtr being serialized.</param>
	/// <param name="node">A <see cref="JsonNode"/> containing the serialized data.</param>
	/// <returns>True if successfully serialized.</returns>
	bool TrySerialize<TPPtr, TAsset>(TPPtr pptr, SerializationOptions options, [NotNullWhen(true)] out JsonNode? node)
		where TPPtr : IPPtr<TAsset>
		where TAsset : IUnityObjectBase;
}
