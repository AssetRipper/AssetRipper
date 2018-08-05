using System;
using System.Collections.Generic;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	public interface IAssetExporter
	{
		bool IsHandle(Object asset);

		void Export(IExportContainer container, Object asset, string path);
		void Export(IExportContainer container, Object asset, string path, Action<IExportContainer, Object, string> callback);
		void Export(IExportContainer container, IEnumerable<Object> assets, string path);
		void Export(IExportContainer container, IEnumerable<Object> assets, string path, Action<IExportContainer, Object, string> callback);

		IExportCollection CreateCollection(Object asset);
		AssetType ToExportType(Object asset);
		bool ToUnknownExportType(ClassIDType classID, out AssetType assetType);
	}
}
