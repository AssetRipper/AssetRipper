namespace AssetRipper.Core.Configuration
{
	public enum BundledAssetsExportMode
	{
		/// <summary>
		/// Bundled assets are treated the same as assets from other files
		/// </summary>
		GroupByAssetType,
		/// <summary>
		/// Bundled assets are grouped by their asset bundle name<br/>
		/// For example: Assets/Asset_Bundles/NameOfAssetBundle/InternalPath1/.../InternalPathN/assetName.extension
		/// </summary>
		GroupByBundleName,
		/// <summary>
		/// Bundled assets are exported directed into the Assets folder.<br/>
		/// For example: Assets/InternalPath1/.../InternalPathN/bundledAssetName.extension<br/>
		/// Normal assets are moved into a subfolder with a folder name unlikely to conflict with other folders.<br/>
		/// For example: Assets/NonbundledAssets/PrefabInstance/ExamplePrefab.prefab
		/// </summary>
		DirectExport,
	}
}
