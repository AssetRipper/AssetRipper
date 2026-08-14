using AssetRipper.Assets;

namespace AssetRipper.Export.UnityProjects;

internal static class ExportNameUtilities
{
	public static string GetSafeAssetName(IUnityObjectBase asset)
	{
		string name = FileSystem.RemoveCloneSuffixes(asset.GetBestName());
		name = FileSystem.RemoveInstanceSuffixes(name).Trim();
		return FileSystem.GetSafeFileName(name, GetFallbackName(asset));
	}

	public static string GetSafeCollectionDisplayName(IExportCollection collection)
	{
		IUnityObjectBase? asset = collection.Assets.FirstOrDefault();
		string fallback = asset is null ? collection.GetType().Name : GetFallbackName(asset);
		string safeName = FileSystem.GetSafeFileName(collection.Name, fallback);
		return FileSystem.ContainsUnsafeUnicode(collection.Name) ? $"{safeName} [obfuscated]" : safeName;
	}

	public static string GetFallbackName(IUnityObjectBase asset) => $"{asset.ClassName}_{asset.PathID}";
}
