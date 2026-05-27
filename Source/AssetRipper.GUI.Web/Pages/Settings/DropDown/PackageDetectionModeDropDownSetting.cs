using AssetRipper.Export.Configuration;

namespace AssetRipper.GUI.Web.Pages.Settings.DropDown;

public sealed class PackageDetectionModeDropDownSetting : DropDownSetting<PackageDetectionMode>
{
	public static PackageDetectionModeDropDownSetting Instance { get; } = new();

	public override string Title => Localization.PackageDetectionTitle;

	protected override string GetDisplayName(PackageDetectionMode value) => value switch
	{
		PackageDetectionMode.Off => Localization.PackageDetectionOff,
		PackageDetectionMode.Auto => Localization.PackageDetectionAuto,
		_ => base.GetDisplayName(value),
	};

	protected override string? GetDescription(PackageDetectionMode value) => value switch
	{
		PackageDetectionMode.Off => Localization.PackageDetectionOffDescription,
		PackageDetectionMode.Auto => Localization.PackageDetectionAutoDescription,
		_ => base.GetDescription(value),
	};
}
