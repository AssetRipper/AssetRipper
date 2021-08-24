using AssetRipper.Core.Configuration;

namespace AssetRipper.Library.Configuration
{
	public class LibraryConfiguration : CoreConfiguration
	{
		public AudioExportFormat AudioExportFormat { get; set; } = AudioExportFormat.Ogg;
		public ImageExportFormat ImageExportFormat { get; set; } = ImageExportFormat.Png;
		public MeshExportFormat MeshExportFormat { get; set; } = MeshExportFormat.Obj;
		public ShaderExportMode ShaderExportMode { get; set; } = ShaderExportMode.Dummy;
		public SpriteExportMode SpriteExportMode { get; set; } = SpriteExportMode.Texture2D;
	}
}
