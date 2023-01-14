using AssetRipper.Export.UnityProjects.Configuration;

namespace AssetRipper.GUI.Components
{
	public class TextExportConfigDropdown : BaseConfigurationDropdown<TextExportMode>
	{
		protected override string GetValueDisplayName(TextExportMode value) => value switch
		{
			TextExportMode.Bytes => MainWindow.Instance.LocalizationManager["text_asset_format_binary"],
			TextExportMode.Txt => MainWindow.Instance.LocalizationManager["text_asset_format_text"],
			TextExportMode.Parse => MainWindow.Instance.LocalizationManager["text_asset_format_parse"],
			_ => base.GetValueDisplayName(value),
		};

		protected override string? GetValueDescription(TextExportMode value) => value switch
		{
			TextExportMode.Bytes => MainWindow.Instance.LocalizationManager["text_asset_format_binary_description"],
			TextExportMode.Txt => MainWindow.Instance.LocalizationManager["text_asset_format_text_description"],
			TextExportMode.Parse => MainWindow.Instance.LocalizationManager["text_asset_format_parse_description"],
			_ => null,
		};
	}
}
