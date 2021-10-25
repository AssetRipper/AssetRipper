using AssetRipper.Core;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Structure.Assembly.Managers;
using AssetRipper.Library.Configuration;
using System;
using System.Collections.Generic;
using Object = AssetRipper.Core.Classes.Object.Object;

namespace AssetRipper.Library.Exporters.Scripts
{
	public class AssemblyDllExporter : IAssetExporter
	{
		public IAssemblyManager AssemblyManager { get; }
		public ScriptExportMode ScriptExportMode { get; }

		public AssemblyDllExporter(IAssemblyManager assemblyManager, LibraryConfiguration configuration)
		{
			AssemblyManager = assemblyManager;
			ScriptExportMode = configuration.ScriptExportMode;
		}

		public bool IsHandle(UnityObjectBase asset)
		{
			return ScriptExportMode == ScriptExportMode.Package;
		}

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObjectBase asset)
		{
			return new AssemblyExportCollection(this, (MonoScript)asset);
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
			throw new NotSupportedException("Assemblies are exported inside the export collection");
		}

		public void Export(IExportContainer container, IEnumerable<UnityObjectBase> assets, string dirPath, Action<IExportContainer, UnityObjectBase, string> callback)
		{
			throw new NotSupportedException("Assemblies are exported inside the export collection");
		}

		public AssetType ToExportType(UnityObjectBase asset) => AssetType.Meta;

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}
	}
}