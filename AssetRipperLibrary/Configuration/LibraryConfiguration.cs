using AssetRipper.Core.Configuration;

namespace AssetRipper.Library.Configuration
{
	public class LibraryConfiguration : CoreConfiguration
	{
		public ShaderExportMode ShaderExportMode { get; set; } = ShaderExportMode.Dummy;
		public SpriteExportMode SpriteExportMode { get; set; } = SpriteExportMode.Texture2D;
	}
}
