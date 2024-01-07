using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;

namespace AssetRipper.SourceGenerated.Extensions;

public static class PPtrExtensions
{
	public static T? TryGetAsset<T>(this IPPtr<T> pptr, AssetCollection file) where T : IUnityObjectBase
	{
		pptr.TryGetAsset(file, out T? asset);
		return asset;
	}

	public static bool IsAsset<T>(this IPPtr<T> pptr, AssetCollection file, IUnityObjectBase asset) where T : IUnityObjectBase
	{
		if (asset.PathID != pptr.PathID)
		{
			return false;
		}
		else if (pptr.FileID == 0)
		{
			return file == asset.Collection;
		}
		else
		{
			return file.Dependencies[pptr.FileID - 1] == asset.Collection;
		}
	}

	/// <summary>
	/// PathID == 0
	/// </summary>
	public static bool IsNull(this IPPtr pptr) => pptr.PathID == 0;
}
