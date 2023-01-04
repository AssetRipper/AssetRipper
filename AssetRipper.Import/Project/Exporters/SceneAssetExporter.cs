using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Import.Project.Collections;
using AssetRipper.IO.Files;
using AssetRipper.SourceGenerated.Classes.ClassID_1032;

namespace AssetRipper.Import.Project.Exporters
{
	public sealed class SceneAssetExporter : IAssetExporter
	{
		public IExportCollection CreateCollection(TemporaryAssetCollection temporaryFile, IUnityObjectBase asset)
		{
			return new SceneAssetExportCollection((ISceneAsset)asset);
		}

		public bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, IUnityObjectBase asset, string path, Action<IExportContainer, IUnityObjectBase, string> callback)
		{
			throw new NotSupportedException();
		}

		public bool Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path, Action<IExportContainer, IUnityObjectBase, string> callback)
		{
			throw new NotSupportedException();
		}

		public bool IsHandle(IUnityObjectBase asset)
		{
			return asset is ISceneAsset;
		}

		public AssetType ToExportType(IUnityObjectBase asset)
		{
			return AssetType.Meta;
		}

		public bool ToUnknownExportType(Type type, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}
	}
}
