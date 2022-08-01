using AssetRipper.Core.Configuration;

namespace AssetRipper.GUI.Components
{
	public class BundledAssetsExportConfigDropdown : BaseConfigurationDropdown<BundledAssetsExportMode>
	{
		protected override string GetValueDisplayName(BundledAssetsExportMode value) => value switch
		{
			BundledAssetsExportMode.GroupByAssetType => MainWindow.Instance.LocalizationManager["bundled_assets_export_group_by_asset_type"],
			BundledAssetsExportMode.GroupByBundleName => MainWindow.Instance.LocalizationManager["bundled_assets_export_group_by_bundle_name"],
			BundledAssetsExportMode.DirectExport => MainWindow.Instance.LocalizationManager["bundled_assets_export_direct_export"],
			_ => base.GetValueDisplayName(value),
		};

		protected override string? GetValueDescription(BundledAssetsExportMode value) => value switch
		{
			BundledAssetsExportMode.GroupByAssetType => MainWindow.Instance.LocalizationManager["bundled_assets_export_group_by_asset_type_description"],
			BundledAssetsExportMode.GroupByBundleName => MainWindow.Instance.LocalizationManager["bundled_assets_export_group_by_bundle_name_description"],
			BundledAssetsExportMode.DirectExport => MainWindow.Instance.LocalizationManager["bundled_assets_export_direct_export_description"],
			_ => null,
		};
	}
}
