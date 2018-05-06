using System.Collections.Generic;
using UtinyRipper.Classes;

namespace UtinyRipper.AssetExporters
{
	public interface IAssetExporter
	{
		void Export(ProjectAssetContainer container, Object asset, string path);
		void Export(ProjectAssetContainer container, IEnumerable<Object> assets, string path);

		IExportCollection CreateCollection(Object asset);
		AssetType ToExportType(ClassIDType classID);
	}
}
