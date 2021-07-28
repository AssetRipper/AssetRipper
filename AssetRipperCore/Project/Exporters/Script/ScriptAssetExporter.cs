using AssetRipper.Project.Exporters.Script.Elements;
using AssetRipper.Parser.Asset;
using AssetRipper.Classes;
using AssetRipper.Parser.Files.SerializedFiles;
using AssetRipper.Structure.Collections;
using System;
using System.Collections.Generic;
using UnityObject = AssetRipper.Classes.Object.UnityObject;

namespace AssetRipper.Project.Exporters.Script
{
	public class ScriptAssetExporter : IAssetExporter
	{
		public bool IsHandle(UnityObject asset, ExportOptions options)
		{
			return true;
		}

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObject asset)
		{
			return new ScriptExportCollection(this, (MonoScript)asset);
		}

		public bool Export(IExportContainer container, UnityObject asset, string path)
		{
			throw new NotSupportedException("Need to export all scripts at once");
		}

		public void Export(IExportContainer container, UnityObject asset, string path, Action<IExportContainer, UnityObject, string> callback)
		{
			throw new NotSupportedException("Need to export all scripts at once");
		}

		public bool Export(IExportContainer container, IEnumerable<UnityObject> assets, string dirPath)
		{
			Export(container, assets, dirPath, null);
			return true;
		}

		public void Export(IExportContainer container, IEnumerable<UnityObject> assets, string dirPath, Action<IExportContainer, UnityObject, string> callback)
		{
			ScriptExportManager scriptManager = new ScriptExportManager(container.Layout, dirPath);
			Dictionary<UnityObject, ScriptExportType> exportTypes = new Dictionary<UnityObject, ScriptExportType>();
			foreach (UnityObject asset in assets)
			{
				MonoScript script = (MonoScript)asset;
				ScriptExportType exportType = script.GetExportType(scriptManager);
				exportTypes.Add(asset, exportType);
			}
			foreach (KeyValuePair<UnityObject, ScriptExportType> exportType in exportTypes)
			{
				string path = scriptManager.Export(exportType.Value);
				if (path != null)
				{
					callback?.Invoke(container, exportType.Key, path);
				}
			}
			scriptManager.ExportRest();
		}

		public AssetType ToExportType(UnityObject asset)
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
