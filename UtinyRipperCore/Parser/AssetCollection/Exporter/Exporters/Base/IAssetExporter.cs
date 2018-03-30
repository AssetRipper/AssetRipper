using UtinyRipper.Classes;

namespace UtinyRipper.AssetExporters
{
	public interface IAssetExporter
	{
		IExportCollection CreateCollection(Object @object);
		bool Export(IAssetsExporter exporter, IExportCollection collection, string dirPath);
		AssetType ToExportType(ClassIDType classID);
	}
}
