using AssetRipper.Assets.Metadata;
using System.Text.Json.Nodes;

namespace AssetRipper.Assets.IO.Serialization;

/// <summary>
/// A default implementation of <see cref="IUnityAssetDeserializer"/> that does nothing.
/// </summary>
public class DefaultUnityAssetDeserializer : IUnityAssetDeserializer
{
	public static DefaultUnityAssetDeserializer Shared { get; } = new();
	public virtual bool TryDeserialize<T>(T asset, JsonNode node, DeserializationOptions options)
		where T : IUnityAssetBase
	{
		return false;
	}

	public virtual bool TryDeserialize<TPPtr, TAsset>(TPPtr pptr, JsonNode node, DeserializationOptions options)
		where TPPtr : IPPtr<TAsset>
		where TAsset : IUnityObjectBase
	{
		return false;
	}
}
