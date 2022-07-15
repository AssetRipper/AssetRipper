using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using System.Collections.Generic;

namespace AssetRipper.Core.Project.Exporters
{
	public interface IAssetExporter
	{
		bool IsHandle(IUnityObjectBase asset);

		bool Export(IExportContainer container, IUnityObjectBase asset, string path);
		void Export(IExportContainer container, IUnityObjectBase asset, string path, Action<IExportContainer, IUnityObjectBase, string> callback);
		bool Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path);
		void Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path, Action<IExportContainer, IUnityObjectBase, string> callback);

		IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset);
		AssetType ToExportType(IUnityObjectBase asset);
		bool ToUnknownExportType(Type type, out AssetType assetType);
	}
}
