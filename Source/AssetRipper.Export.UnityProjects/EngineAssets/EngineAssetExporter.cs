using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Import.Configuration;
using AssetRipper.IO.Files;

namespace AssetRipper.Export.UnityProjects.EngineAssets
{
	public class EngineAssetExporter : IAssetExporter
	{
		private UnityVersion Version { get; }
		public EngineAssetExporter(CoreConfiguration configuration) => Version = configuration.Version;

		public bool TryCreateCollection(IUnityObjectBase asset, TemporaryAssetCollection temporaryFile, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			if (EngineExportCollection.IsEngineAsset(asset, Version))
			{
				exportCollection = new EngineExportCollection(asset, temporaryFile.Version);
				return true;
			}
			else
			{
				exportCollection = null;
				return false;
			}
		}

		public bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, IUnityObjectBase asset, string path, Action<IExportContainer, IUnityObjectBase, string>? callback)
		{
			throw new NotSupportedException();
		}

		public bool Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path, Action<IExportContainer, IUnityObjectBase, string>? callback)
		{
			throw new NotSupportedException();
		}

		public AssetType ToExportType(IUnityObjectBase asset)
		{
			return AssetType.Internal;
		}

		public bool ToUnknownExportType(Type type, out AssetType assetType)
		{
			assetType = AssetType.Internal;
			return false;
		}
	}
}
