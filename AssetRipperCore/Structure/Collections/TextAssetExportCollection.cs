using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Object;

namespace AssetRipper.Core.Structure.Collections
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
