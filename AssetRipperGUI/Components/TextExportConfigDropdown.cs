using AssetRipper.Library.Configuration;

namespace AssetRipper.GUI.Components
{
	public class TextExportConfigDropdown : BaseConfigurationDropdown<TextExportMode>
	{

		protected override string? GetValueDescription(TextExportMode value)  => value switch
		{
			TextExportMode.Bytes => "Export the raw bytes of the text asset with a .bytes extension.",
			TextExportMode.Txt => "Export as a plain text file (.txt)",
			TextExportMode.Parse => "Export as a plain text file, but try to guess the correct file extension (e.g. JSON files get the .json extension)",
			_ => null,
		};
	}
}