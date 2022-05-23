﻿using AssetRipper.Core;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Structure.Assembly.Managers;
using AssetRipper.Library.Configuration;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using System;
using System.Collections.Generic;

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

		public bool IsHandle(IUnityObjectBase asset)
		{
			return ScriptExportMode == ScriptExportMode.DllExportWithoutRenaming && asset is IMonoScript;
		}

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new AssemblyExportCollection(this, (IMonoScript)asset);
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
			throw new NotSupportedException("Assemblies are exported inside the export collection");
		}

		public void Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string dirPath, Action<IExportContainer, IUnityObjectBase, string> callback)
		{
			throw new NotSupportedException("Assemblies are exported inside the export collection");
		}

		public AssetType ToExportType(IUnityObjectBase asset) => AssetType.Meta;

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}
	}
}
