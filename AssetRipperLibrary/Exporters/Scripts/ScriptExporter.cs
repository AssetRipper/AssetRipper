using AssetRipper.Core;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Structure.Assembly.Managers;
using AssetRipper.Library.Configuration;
using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace AssetRipper.Library.Exporters.Scripts
{
	public class ScriptExporter : IAssetExporter
	{
		private IAssemblyManager AssemblyManager { get; }
		public ScriptExportMode ScriptExportMode { get; }
		public ICSharpCode.Decompiler.CSharp.LanguageVersion LanguageVersion { get; }

		public ScriptExporter(IAssemblyManager assemblyManager, LibraryConfiguration configuration)
		{
			AssemblyManager = assemblyManager;
			ScriptExportMode = configuration.ScriptExportMode;
			LanguageVersion = configuration.ScriptLanguageVersion;
		}

		public bool IsHandle(IUnityObjectBase asset)
		{
			return ScriptExportMode == ScriptExportMode.Decompiled && asset is IMonoScript;
		}

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new ScriptExportCollection(this, (MonoScript)asset);
		}

		public bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			throw new NotSupportedException("Need to export all scripts at once");
		}

		public void Export(IExportContainer container, IUnityObjectBase asset, string path, Action<IExportContainer, IUnityObjectBase, string> callback)
		{
			throw new NotSupportedException("Need to export all scripts at once");
		}

		public bool Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string dirPath)
		{
			Export(container, assets, dirPath, null);
			return true;
		}

		public void Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string dirPath, Action<IExportContainer, IUnityObjectBase, string> callback)
		{
			Logger.Info(LogCategory.Export, "Exporting scripts...");
			ScriptManager scriptManager = new ScriptManager(AssemblyManager, dirPath, LanguageVersion);
			Dictionary<IUnityObjectBase, TypeDefinition> exportTypes = new Dictionary<IUnityObjectBase, TypeDefinition>();
			foreach (IUnityObjectBase asset in assets)
			{
				IMonoScript script = (IMonoScript)asset;
				TypeDefinition exportType = script.GetTypeDefinition();
				exportTypes.Add(asset, exportType);
			}
			int primaryTotal = exportTypes.Count;
			int count = 0;
			Logger.Info(LogCategory.Export, $"Exporting {primaryTotal} primary scripts...");
			foreach (KeyValuePair<IUnityObjectBase, TypeDefinition> exportType in exportTypes)
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

		public AssetType ToExportType(IUnityObjectBase asset) => AssetType.Meta;

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}
	}
}