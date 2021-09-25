using AssetRipper.Core;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Structure.Assembly.Managers;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using Object = AssetRipper.Core.Classes.Object.Object;

namespace AssetRipper.Library.Exporters.Scripts
{
	public class ScriptExporter : IAssetExporter
	{
		private IAssemblyManager AssemblyManager { get; }

		public ScriptExporter(IAssemblyManager assemblyManager)
		{
			AssemblyManager = assemblyManager;
		}

		public bool IsHandle(Object asset)
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
			Logger.Info(LogCategory.Export, "Exporting scripts...");
			ScriptManager scriptManager = new ScriptManager(AssemblyManager, dirPath);
			Dictionary<Object, TypeDefinition> exportTypes = new Dictionary<Object, TypeDefinition>();
			foreach (Object asset in assets)
			{
				MonoScript script = (MonoScript)asset;
				TypeDefinition exportType = script.GetTypeDefinition();
				exportTypes.Add(asset, exportType);
			}
			int primaryTotal = exportTypes.Count;
			int count = 0;
			Logger.Info(LogCategory.Export, $"Exporting {primaryTotal} primary scripts...");
			foreach (KeyValuePair<Object, TypeDefinition> exportType in exportTypes)
			{
				string path = scriptManager.Export(exportType.Value);
				if (path != null)
				{
					callback?.Invoke(container, exportType.Key, path);
				}
				count++;
				if(count % 100 == 0)
				{
					Logger.Info(LogCategory.Export, $"Exported {count}/{primaryTotal} primary scripts");
				}
			}
			Logger.Info(LogCategory.Export, "Primary script export finished. Exporting the rest...");
			scriptManager.ExportRest();
			Logger.Info(LogCategory.Export, "Finished exporting scripts");
		}

		public AssetType ToExportType(Object asset) => AssetType.Meta;

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}
	}
}