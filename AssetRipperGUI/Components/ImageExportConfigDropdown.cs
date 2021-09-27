using AssetRipper.Library.Configuration;

namespace AssetRipper.GUI.Components
{
	public class ImageExportConfigDropdown : BaseConfigurationDropdown<ImageExportFormat>
	{
		protected override string? GetValueDescription(ImageExportFormat value)  => value switch
		{
			_ => "", //No descriptions here
		};
	}
}