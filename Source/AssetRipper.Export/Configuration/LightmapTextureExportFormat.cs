namespace AssetRipper.Export.Configuration;

public enum LightmapTextureExportFormat
{
	/// <summary>
	/// Use <see cref="ImageExportFormat.Exr"/>
	/// </summary>
	Exr,
	/// <summary>
	/// Use <see cref="ExportSettings.ImageExportFormat"/>
	/// </summary>
	Image,
	/// <summary>
	/// The internal Unity format
	/// </summary>
	Yaml,
}
