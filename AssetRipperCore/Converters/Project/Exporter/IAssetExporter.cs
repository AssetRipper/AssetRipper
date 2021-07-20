using AssetRipper.Parser.Asset;
using AssetRipper.Parser.Files.SerializedFile;
using AssetRipper.Structure.ProjectCollection.Collections;
using System;
using System.Collections.Generic;
using Object = AssetRipper.Parser.Classes.Object.Object;

namespace AssetRipper.Converters.Project.Exporter
{
	public interface IAssetExporter
	{
		bool IsHandle(Object asset, ExportOptions options);

		bool Export(IExportContainer container, Object asset, string path);
		void Export(IExportContainer container, Object asset, string path, Action<IExportContainer, Object, string> callback);
		bool Export(IExportContainer container, IEnumerable<Object> assets, string path);
		void Export(IExportContainer container, IEnumerable<Object> assets, string path, Action<IExportContainer, Object, string> callback);

		IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset);
		AssetType ToExportType(Object asset);
		bool ToUnknownExportType(ClassIDType classID, out AssetType assetType);
	}
}
