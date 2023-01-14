using AssetRipper.Export.UnityProjects.Configuration;

namespace AssetRipper.GUI.Components
{
	public class ImageExportConfigDropdown : BaseConfigurationDropdown<ImageExportFormat>
	{
		protected override string? GetValueDescription(ImageExportFormat value) => value switch
		{
			_ => MainWindow.Instance.LocalizationManager["image_format_description"],
		};
	}
}
