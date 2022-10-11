using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Project.Collections;
using AssetRipper.IO.Files;
using System.Collections.Generic;

namespace AssetRipper.Core.Project.Exporters
{
	public sealed class SceneAssetExporter : IAssetExporter
	{
		public IExportCollection CreateCollection(TemporaryAssetCollection temporaryFile, IUnityObjectBase asset)
		{
			return new SceneAssetExportCollection((SceneAsset)asset);
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
			return asset is SceneAsset;
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
