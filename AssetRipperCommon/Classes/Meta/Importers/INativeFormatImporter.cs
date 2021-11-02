using AssetRipper.Core.Classes.Meta.Importers.Asset;

namespace AssetRipper.Core.Classes.Meta.Importers
{
	public interface INativeFormatImporter : IAssetImporter
	{
		public long MainObjectFileID { get; set; }
	}
}
