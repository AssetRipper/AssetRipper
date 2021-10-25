using AssetRipper.Core.Configuration;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using System;
using System.Collections.Generic;
using Object = AssetRipper.Core.Classes.Object.Object;

namespace AssetRipper.Core.Project.Exporters.Engine
{
	public class EngineAssetExporter : IAssetExporter
	{
		private UnityVersion Version { get; }
		public EngineAssetExporter(CoreConfiguration configuration) => Version = configuration.Version;

		public bool IsHandle(UnityObjectBase asset)
		{
			return EngineExportCollection.IsEngineAsset(asset, Version);
		}

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new EngineExportCollection(asset, virtualFile.Version);
		}

		public bool Export(IExportContainer container, Object asset, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, Object asset, string path, Action<IExportContainer, Object, string> callback)
		{
			throw new NotSupportedException();
		}

		public bool Export(IExportContainer container, IEnumerable<Object> assets, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path, Action<IExportContainer, Object, string> callback)
		{
			throw new NotSupportedException();
		}

		public AssetType ToExportType(UnityObjectBase asset)
		{
			return AssetType.Internal;
		}

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			assetType = AssetType.Internal;
			return false;
		}
	}
}
