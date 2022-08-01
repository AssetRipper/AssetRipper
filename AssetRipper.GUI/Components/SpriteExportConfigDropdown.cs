using AssetRipper.Library.Configuration;

namespace AssetRipper.GUI.Components
{
	public class SpriteExportConfigDropdown : BaseConfigurationDropdown<SpriteExportMode>
	{
		protected override string GetValueDisplayName(SpriteExportMode value) => value switch
		{
			SpriteExportMode.Native => MainWindow.Instance.LocalizationManager["sprite_format_native"],
			SpriteExportMode.Texture2D => MainWindow.Instance.LocalizationManager["sprite_format_texture"],
			_ => base.GetValueDisplayName(value),
		};

		protected override string? GetValueDescription(SpriteExportMode value) => value switch
		{
			SpriteExportMode.Native => MainWindow.Instance.LocalizationManager["sprite_format_native_description"],
			SpriteExportMode.Texture2D => MainWindow.Instance.LocalizationManager["sprite_format_texture_description"],
			_ => null,
		};
	}
}