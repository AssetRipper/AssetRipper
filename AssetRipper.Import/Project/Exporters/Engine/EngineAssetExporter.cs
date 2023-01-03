using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Core.Configuration;
using AssetRipper.Core.Project.Collections;
using AssetRipper.IO.Files;
using System.Collections.Generic;

namespace AssetRipper.Core.Project.Exporters.Engine
{
	public class EngineAssetExporter : IAssetExporter
	{
		private UnityVersion Version { get; }
		public EngineAssetExporter(CoreConfiguration configuration) => Version = configuration.Version;

		public bool IsHandle(IUnityObjectBase asset)
		{
			return EngineExportCollection.IsEngineAsset(asset, Version);
		}

		public IExportCollection CreateCollection(TemporaryAssetCollection virtualFile, IUnityObjectBase asset)
		{
			return new EngineExportCollection(asset, virtualFile.Version);
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
