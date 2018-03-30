using System.Collections.Generic;
using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;

namespace UtinyRipper.AssetExporters
{
	public interface IExportCollection
	{
		bool IsContains(Object @object);
		string GetExportID(Object @object);
		ExportPointer CreateExportPointer(Object @object, bool isLocal);

		IAssetExporter AssetExporter { get; }
		IEnumerable<Object> Objects { get; }
		string Name { get; }
		UtinyGUID GUID { get; }
		IYAMLExportable MetaImporter { get; }
	}
}
