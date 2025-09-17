using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;

#nullable disable

namespace AssetRipper.AssemblyDumper.InjectedTypes;

internal static class PPtrHelper
{
	public static PPtr ForceCreatePPtr(AssetCollection collection, IUnityObjectBase asset)
	{
		return collection.ForceCreatePPtr(asset);
	}
	public static bool TryGetAsset<T>(AssetCollection collection, int fileID, long pathID, [NotNullWhen(true)] out T asset) where T : IUnityObjectBase
	{
		return collection.TryGetAsset(fileID, pathID, out asset);
	}
}

#nullable enable