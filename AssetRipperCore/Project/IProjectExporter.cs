using AssetRipper.Core.Configuration;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Structure;
using System.Collections.Generic;

namespace AssetRipper.Core.Project
{
	public interface IProjectExporter
	{
		void Export(GameCollection fileCollection, CoreConfiguration options);
		void Export(GameCollection fileCollection, IEnumerable<SerializedFile> files, CoreConfiguration options);
		void Export(GameCollection fileCollection, SerializedFile file, CoreConfiguration options);
		void OverrideDummyExporter(ClassIDType classType, bool isEmptyCollection, bool isMetaType);
		void OverrideExporter(ClassIDType classType, IAssetExporter exporter);
		void OverrideYamlExporter(ClassIDType classType);
		AssetType ToExportType(ClassIDType classID);
	}
}