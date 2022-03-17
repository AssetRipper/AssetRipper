using AssetRipper.Core.Classes.Meta.Importers.Asset;

namespace AssetRipper.Core.Classes.Meta.Importers
{
	public interface IMonoImporter : IAssetImporter
	{
		public short ExecutionOrder { get; set; }
	}
}
