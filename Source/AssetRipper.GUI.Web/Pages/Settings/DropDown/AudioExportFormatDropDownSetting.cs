using AssetRipper.Export.Configuration;

namespace AssetRipper.GUI.Web.Pages.Settings.DropDown;

public sealed class AudioExportFormatDropDownSetting : DropDownSetting<AudioExportFormat>
{
	public static AudioExportFormatDropDownSetting Instance { get; } = new();

	public override string Title => Localization.AudioExportTitle;

	protected override string GetDisplayName(AudioExportFormat value) => value switch
	{
		AudioExportFormat.Yaml => Localization.AudioFormatYaml,
		AudioExportFormat.Native => Localization.AudioFormatNative,
		AudioExportFormat.PreferWav => Localization.AudioFormatForceWav,
		AudioExportFormat.Default => Localization.AudioFormatDefault,
		_ => base.GetDisplayName(value),
	};

	protected override string? GetDescription(AudioExportFormat value) => value switch
	{
		AudioExportFormat.Yaml => Localization.AudioFormatYamlDescription,
		AudioExportFormat.Native => Localization.AudioFormatNativeDescription,
		AudioExportFormat.PreferWav => Localization.AudioFormatForceWavDescription,
		AudioExportFormat.Default => Localization.AudioFormatDefaultDescription,
		_ => base.GetDescription(value),
	};
}
