using uTinyRipper.Project;
using uTinyRipper.Classes;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Converters
{
	public sealed class FontAssetExporter : BinaryAssetExporter
	{
		public override bool IsHandle(Object asset, ExportOptions options)
		{
			Font font = (Font)asset;
			return font.IsValidData;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new FontExportCollection(this, (Font)asset);
		}
	}
}
