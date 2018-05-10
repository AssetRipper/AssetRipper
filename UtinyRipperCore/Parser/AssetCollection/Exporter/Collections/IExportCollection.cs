using System.Collections.Generic;
using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;

namespace UtinyRipper.AssetExporters
{
	public interface IExportCollection
	{
		bool Export(ProjectAssetContainer container, string dirPath);
		bool IsContains(Object asset);
		string GetExportID(Object asset);
		ExportPointer CreateExportPointer(Object asset, bool isLocal);

		IEnumerable<Object> Objects { get; }
		string Name { get; }
	}
}
