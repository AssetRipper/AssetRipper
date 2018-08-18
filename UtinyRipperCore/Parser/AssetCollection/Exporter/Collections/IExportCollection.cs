using System.Collections.Generic;
using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.AssetExporters
{
	public interface IExportCollection
	{
		bool Export(ProjectAssetContainer container, string dirPath);
		bool IsContains(Object asset);
		long GetExportID(Object asset);
		ExportPointer CreateExportPointer(Object asset, bool isLocal);

		ISerializedFile File { get; }
		IEnumerable<Object> Assets { get; }
		string Name { get; } 
	}
}
