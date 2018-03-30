using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.AssetExporters
{
	public interface IAssetsExporter
	{
		string GetExportID(Object @object);
		AssetType ToExportType(ClassIDType classID);
		ExportPointer CreateExportPointer(Object @object);

		ISerializedFile File { get; set; }
		Version Version { get; }
		Platform Platform { get; }
	}
}
