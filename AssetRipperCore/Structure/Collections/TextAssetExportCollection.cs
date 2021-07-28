using AssetRipper.Project.Exporters;
using AssetRipper.Classes;
using AssetRipper.Classes.Object;

namespace AssetRipper.Structure.Collections
{
	public sealed class TextAssetExportCollection : AssetExportCollection
	{
		public TextAssetExportCollection(IAssetExporter assetExporter, TextAsset asset) : base(assetExporter, asset) { }

		protected override string GetExportExtension(Object asset)
		{
			return "bytes";
		}
	}
}
