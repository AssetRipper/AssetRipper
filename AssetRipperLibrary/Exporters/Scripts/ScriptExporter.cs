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

		public bool IsHandle(UnityObjectBase asset)
		{
			return ScriptExportMode == ScriptExportMode.Decompiled;
		}

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObjectBase asset)
		{
			return new ScriptExportCollection(this, (MonoScript)asset);
		}

		public bool Export(IExportContainer container, UnityObjectBase asset, string path)
		{
			throw new NotSupportedException("Need to export all scripts at once");
		}

		public void Export(IExportContainer container, UnityObjectBase asset, string path, Action<IExportContainer, UnityObjectBase, string> callback)
		{
			throw new NotSupportedException("Need to export all scripts at once");
		}

		public bool Export(IExportContainer container, IEnumerable<UnityObjectBase> assets, string dirPath)
		{
			Export(container, assets, dirPath, null);
			return true;
		}

		public void Export(IExportContainer container, IEnumerable<UnityObjectBase> assets, string dirPath, Action<IExportContainer, UnityObjectBase, string> callback)
		{
			Logger.Info(LogCategory.Export, "Exporting scripts...");
			ScriptManager scriptManager = new ScriptManager(AssemblyManager, dirPath, LanguageVersion);
			Dictionary<UnityObjectBase, TypeDefinition> exportTypes = new Dictionary<UnityObjectBase, TypeDefinition>();
			foreach (UnityObjectBase asset in assets)
			{
				MonoScript script = (MonoScript)asset;
				TypeDefinition exportType = script.GetTypeDefinition();
				exportTypes.Add(asset, exportType);
			}
			int primaryTotal = exportTypes.Count;
			int count = 0;
			Logger.Info(LogCategory.Export, $"Exporting {primaryTotal} primary scripts...");
			foreach (KeyValuePair<UnityObjectBase, TypeDefinition> exportType in exportTypes)
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

		public AssetType ToExportType(UnityObjectBase asset) => AssetType.Meta;

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}
	}
}