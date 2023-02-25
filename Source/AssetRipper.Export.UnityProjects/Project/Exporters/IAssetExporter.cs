using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.IO.Files;

namespace AssetRipper.Export.UnityProjects.Project.Exporters
{
	public interface IAssetExporter
	{
		bool TryCreateCollection(IUnityObjectBase asset, TemporaryAssetCollection temporaryFile, [NotNullWhen(true)] out IExportCollection? exportCollection);

		bool Export(IExportContainer container, IUnityObjectBase asset, string path);
		void Export(IExportContainer container, IUnityObjectBase asset, string path, Action<IExportContainer, IUnityObjectBase, string>? callback);
		bool Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path);
		void Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path, Action<IExportContainer, IUnityObjectBase, string>? callback);

		AssetType ToExportType(IUnityObjectBase asset);
		bool ToUnknownExportType(Type type, out AssetType assetType);
	}
}
