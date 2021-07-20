using AssetRipper.Converters.Project.Exporter;
using AssetRipper.Parser.Classes;
using AssetRipper.Parser.Classes.Object;

namespace AssetRipper.Structure.ProjectCollection.Collections
{
	public sealed class TextAssetExportCollection : AssetExportCollection
	{
		public TextAssetExportCollection(IAssetExporter assetExporter, TextAsset asset) :
			base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(Object asset)
		{
			return "bytes";
		}
	}
}
