using AssetRipper.Core;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Configuration;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using System;
using System.Collections.Generic;

namespace AssetRipper.Library.Exporters.Scripts
{
	public class SkipScriptExporter : IAssetExporter
	{
		private readonly ScriptContentLevel level;
		public SkipScriptExporter(CoreConfiguration configuration)
		{
			level = configuration.ScriptContentLevel;
		}

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new EmptyExportCollection();
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
			return asset is IMonoScript && level == ScriptContentLevel.Level0;
		}

		public AssetType ToExportType(IUnityObjectBase asset)
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
