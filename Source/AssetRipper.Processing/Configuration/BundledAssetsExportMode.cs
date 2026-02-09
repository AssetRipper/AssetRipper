namespace AssetRipper.Processing.Configuration;

public enum BundledAssetsExportMode
{
	/// <summary>
	/// Bundled assets are treated the same as assets from other files.
	/// </summary>
	GroupByAssetType,
	/// <summary>
	/// Bundled assets are grouped by their asset bundle name.<br/>
	/// For example: Assets/Asset_Bundles/NameOfAssetBundle/InternalPath1/.../InternalPathN/assetName.extension
	/// </summary>
	GroupByBundleName,
	/// <summary>
	/// Bundled assets are exported without grouping.<br/>
	/// For example: Assets/InternalPath1/.../InternalPathN/bundledAssetName.extension
	/// </summary>
	DirectExport,
}
