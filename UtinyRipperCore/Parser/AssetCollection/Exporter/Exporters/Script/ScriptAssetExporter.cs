using System;
using System.Collections.Generic;
using UtinyRipper.Classes;
using UtinyRipper.Exporters.Scripts;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	public class ScriptAssetExporter : IAssetExporter
	{
		public bool IsHandle(Object asset)
		{
			return true;
		}

		public IExportCollection CreateCollection(Object asset)
		{
			return new ScriptExportCollection(this, (MonoScript)asset);
		}

		public void Export(IExportContainer container, Object asset, string path)
		{
			Export(container, asset, path);
		}

		public void Export(IExportContainer container, Object asset, string path, Action<IExportContainer, Object, string> callback)
		{
			throw new NotSupportedException("You have to export all script at once");
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string dirPath)
		{
			Export(container, assets, dirPath, null);
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string dirPath, Action<IExportContainer, Object, string> callback)
		{
			ScriptExportManager scriptManager = new ScriptExportManager(dirPath);
			Dictionary<Object, ScriptExportType> exportTypes = new Dictionary<Object, ScriptExportType>();
			foreach (Object asset in assets)
			{
				MonoScript script = (MonoScript)asset;
				ScriptExportType exportType = script.CreateExportType(scriptManager);
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
