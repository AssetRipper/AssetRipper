using AssetRipper.Parser.Asset;
using AssetRipper.Parser.Files.SerializedFiles;
using AssetRipper.Structure.Collections;
using System;
using System.Collections.Generic;
using UnityObject = AssetRipper.Classes.Object.UnityObject;

namespace AssetRipper.Project.Exporters
{
	public interface IAssetExporter
	{
		bool IsHandle(UnityObject asset, ExportOptions options);

		bool Export(IExportContainer container, UnityObject asset, string path);
		void Export(IExportContainer container, UnityObject asset, string path, Action<IExportContainer, UnityObject, string> callback);
		bool Export(IExportContainer container, IEnumerable<UnityObject> assets, string path);
		void Export(IExportContainer container, IEnumerable<UnityObject> assets, string path, Action<IExportContainer, UnityObject, string> callback);

		IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObject asset);
		AssetType ToExportType(UnityObject asset);
		bool ToUnknownExportType(ClassIDType classID, out AssetType assetType);
	}
}
