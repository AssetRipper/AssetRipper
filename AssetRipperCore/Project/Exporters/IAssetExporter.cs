using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Project.Exporters
{
	public interface IAssetExporter
	{
		bool IsHandle(UnityObjectBase asset);

		bool Export(IExportContainer container, UnityObjectBase asset, string path);
		void Export(IExportContainer container, UnityObjectBase asset, string path, Action<IExportContainer, UnityObjectBase, string> callback);
		bool Export(IExportContainer container, IEnumerable<UnityObjectBase> assets, string path);
		void Export(IExportContainer container, IEnumerable<UnityObjectBase> assets, string path, Action<IExportContainer, UnityObjectBase, string> callback);

		IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObjectBase asset);
		AssetType ToExportType(UnityObjectBase asset);
		bool ToUnknownExportType(ClassIDType classID, out AssetType assetType);
	}
}
