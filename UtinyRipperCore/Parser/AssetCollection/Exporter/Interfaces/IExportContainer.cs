using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;

namespace UtinyRipper.AssetExporters
{
	public interface IExportContainer
	{
		Object FindObject(int fileIndex, long pathID);
		string GetExportID(Object @object);
		AssetType ToExportType(ClassIDType classID);
		ExportPointer CreateExportPointer(Object @object);

		IExportCollection CurrentCollection { get; }
		Version Version { get; }
		Platform Platform { get; }
		TransferInstructionFlags Flags { get; }
	}
}
