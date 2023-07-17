using AssetRipper.Assets.Metadata;
using System.Text.Json.Nodes;

namespace AssetRipper.Assets.IO.Serialization;

/// <summary>
/// A default implementation of <see cref="IUnityAssetSerializer"/> that does nothing.
/// </summary>
public class DefaultUnityAssetSerializer : IUnityAssetSerializer
{
	public static DefaultUnityAssetSerializer Shared { get; } = new();
	public virtual bool TrySerialize<T>(T asset, SerializationOptions options, [NotNullWhen(true)] out JsonNode? node)
		where T : IUnityAssetBase
	{
		node = default;
		return false;
	}

	public virtual bool TrySerialize<TPPtr, TAsset>(TPPtr pptr, SerializationOptions options, [NotNullWhen(true)] out JsonNode? node)
		where TPPtr : IPPtr<TAsset>
		where TAsset : IUnityObjectBase
	{
		node = default;
		return false;
	}
}
