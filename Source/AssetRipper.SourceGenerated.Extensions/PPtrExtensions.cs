using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;

namespace AssetRipper.SourceGenerated.Extensions;

public static class PPtrExtensions
{
	//Called from source gen
	public static T? TryGetAsset<T>(this IPPtr<T> pptr, AssetCollection file) where T : IUnityObjectBase
	{
		pptr.TryGetAsset(file, out T? asset);
		return asset;
	}

	public static T GetAsset<T>(this IPPtr<T> pptr, IAssetContainer file) where T : IUnityObjectBase
	{
		if (pptr.IsNull())
		{
			throw new Exception("Can't get null PPtr");
		}
		IUnityObjectBase asset = file.GetAsset(pptr.FileID, pptr.PathID);
		if (asset is T t)
		{
			return t;
		}
		throw new Exception($"Object's type {asset.GetType().Name} isn't assignable from {typeof(T).Name}");
	}

	public static bool IsAsset<T>(this IPPtr<T> pptr, IAssetContainer file, IUnityObjectBase asset) where T : IUnityObjectBase
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

	public static bool IsValid<T>(this IPPtr<T> pptr, IExportContainer container) where T : IUnityObjectBase
	{
		return pptr.TryGetAsset(container.File) != null;
	}

	public static string ToLogString<T>(this IPPtr<T> pptr, IAssetContainer container) where T : IUnityObjectBase
	{
		string depName = pptr.FileID == 0 ? container.Name : container.Dependencies[pptr.FileID - 1]?.Name ?? "Null";
		return $"[{depName}]{typeof(T).Name}_{pptr.PathID}";
	}

	/// <summary>
	/// PathID == 0
	/// </summary>
	public static bool IsNull(this IPPtr pptr) => pptr.PathID == 0;
}
