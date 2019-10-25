using uTinyRipper.Classes;

namespace uTinyRipper.AssetExporters
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
