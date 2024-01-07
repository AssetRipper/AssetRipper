using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.IO.Files;

namespace AssetRipper.Export.UnityProjects
{
	public interface IAssetExporter
	{
		bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection);

		bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			Export(container, asset, path, null);
			return true;
		}
		void Export(IExportContainer container, IUnityObjectBase asset, string path, Action<IExportContainer, IUnityObjectBase, string>? callback)
		{
			throw new NotSupportedException();
		}
		bool Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path)
		{
			Export(container, assets, path, null);
			return true;
		}
		void Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path, Action<IExportContainer, IUnityObjectBase, string>? callback)
		{
			throw new NotSupportedException();
		}

		AssetType ToExportType(IUnityObjectBase asset);
		bool ToUnknownExportType(Type type, out AssetType assetType);
	}
}
