using AssetRipper.Library.Configuration;

namespace AssetRipper.GUI.Components
{
	public class SpriteExportConfigDropdown : BaseConfigurationDropdown<SpriteExportMode>
	{
		protected override string GetValueDisplayName(SpriteExportMode value) => value switch
		{
			SpriteExportMode.Native => "Unity",
			SpriteExportMode.Texture2D => "Texture",
			_ => base.GetValueDisplayName(value),
		};

		protected override string? GetValueDescription(SpriteExportMode value)  => value switch
		{
			SpriteExportMode.Native => "Export in the unity sprite format. Cannot be viewed outside of unity.",
			SpriteExportMode.Texture2D => "Export as an image of the Sprite Sheet. Can be viewed outside of unity, but slower to export.",
			_ => null,
		};
	}
}