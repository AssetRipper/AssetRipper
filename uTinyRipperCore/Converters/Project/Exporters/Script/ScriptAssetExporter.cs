using System;
using System.Collections.Generic;
using uTinyRipper.Classes;
using uTinyRipper.Exporters.Scripts;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.AssetExporters
{
	public class ScriptAssetExporter : IAssetExporter
	{
		public bool IsHandle(Object asset, ExportOptions options)
		{
			return true;
		}

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new ScriptExportCollection(this, (MonoScript)asset);
		}

		public bool Export(IExportContainer container, Object asset, string path)
		{
			throw new NotSupportedException("Need to export all scripts at once");
		}

		public void Export(IExportContainer container, Object asset, string path, Action<IExportContainer, Object, string> callback)
		{
			throw new NotSupportedException("Need to export all scripts at once");
		}

		public bool Export(IExportContainer container, IEnumerable<Object> assets, string dirPath)
		{
			Export(container, assets, dirPath, null);
			return true;
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string dirPath, Action<IExportContainer, Object, string> callback)
		{
			ScriptExportManager scriptManager = new ScriptExportManager(dirPath);
			Dictionary<Object, ScriptExportType> exportTypes = new Dictionary<Object, ScriptExportType>();
			foreach (Object asset in assets)
			{
				MonoScript script = (MonoScript)asset;
				ScriptExportType exportType = script.GetExportType(scriptManager);
				exportTypes.Add(asset, exportType);
			}
			foreach (KeyValuePair<Object, ScriptExportType> exportType in exportTypes)
			{
				string path = scriptManager.Export(exportType.Value);
				if (path != null)
				{
					callback?.Invoke(container, exportType.Key, path);
				}
			}
			scriptManager.ExportRest();
		}

		public AssetType ToExportType(Object asset)
		{
			return AssetType.Meta;
		}

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}
	}
}
