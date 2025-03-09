using AssetRipper.Processing.Configuration;

namespace AssetRipper.GUI.Web.Pages.Settings.DropDown;

public sealed class BundledAssetsExportModeDropDownSetting : DropDownSetting<BundledAssetsExportMode>
{
	public static BundledAssetsExportModeDropDownSetting Instance { get; } = new();

	public override string Title => Localization.BundledAssetsExportTitle;

	protected override string GetDisplayName(BundledAssetsExportMode value) => value switch
	{
		BundledAssetsExportMode.GroupByAssetType => Localization.BundledAssetsExportGroupByAssetType,
		BundledAssetsExportMode.GroupByBundleName => Localization.BundledAssetsExportGroupByBundleName,
		BundledAssetsExportMode.DirectExport => Localization.BundledAssetsExportDirectExport,
		_ => base.GetDisplayName(value),
	};

	protected override string? GetDescription(BundledAssetsExportMode value) => value switch
	{
		BundledAssetsExportMode.GroupByAssetType => Localization.BundledAssetsExportGroupByAssetTypeDescription,
		BundledAssetsExportMode.GroupByBundleName => Localization.BundledAssetsExportGroupByBundleNameDescription,
		BundledAssetsExportMode.DirectExport => Localization.BundledAssetsExportDirectExportDescription,
		_ => base.GetDescription(value),
	};
}
