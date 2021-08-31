using AssetRipper.Core.Classes;
using AssetRipper.Core.Configuration;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Structure.Assembly.Managers;
using AssetRipper.Core.Structure.Collections;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using Object = AssetRipper.Core.Classes.Object.Object;

namespace AssetRipper.Core.Project.Exporters.Script
{
	public class AltScriptExporter : IAssetExporter
	{
		private IAssemblyManager AssemblyManager { get; }

		public AltScriptExporter(IAssemblyManager assemblyManager)
		{
			AssemblyManager = assemblyManager;
		}

		public bool IsHandle(Object asset, CoreConfiguration options)
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
			AltScriptManager scriptManager = new AltScriptManager(AssemblyManager, dirPath);
			Dictionary<Object, TypeDefinition> exportTypes = new Dictionary<Object, TypeDefinition>();
			foreach (Object asset in assets)
			{
				MonoScript script = (MonoScript)asset;
				TypeDefinition exportType = script.GetTypeDefinition();
				exportTypes.Add(asset, exportType);
			}
			foreach (KeyValuePair<Object, TypeDefinition> exportType in exportTypes)
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