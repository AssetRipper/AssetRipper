using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;
using UtinyRipper.SerializedFiles;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	public class ProjectAssetContainer : IExportContainer
	{
		public ProjectAssetContainer(ProjectExporter exporter, IEnumerable<Object> assets, IReadOnlyList<IExportCollection> collections)
		{
			if(exporter == null)
			{
				throw new ArgumentNullException(nameof(exporter));
			}
			if (collections == null)
			{
				throw new ArgumentNullException(nameof(collections));
			}
			m_exporter = exporter;
			m_collections = collections;

			foreach (Object asset in assets)
			{
				switch (asset.ClassID)
				{
					case ClassIDType.BuildSettings:
						m_buildSettings = (BuildSettings)asset;
						break;

					case ClassIDType.TagManager:
						m_tagManager = (TagManager)asset;
						break;
				}
			}
		}

		public Object FindObject(int fileIndex, long pathID)
		{
			if(fileIndex == PPtr<Object>.VirtualFileIndex)
			{
				return VirtualFile.FindAsset(pathID);
			}
			else
			{
				return File.FindAsset(fileIndex, pathID);
			}
		}

		public ulong GetExportID(Object asset)
		{
			if (CurrentCollection.IsContains(asset))
			{
				return CurrentCollection.GetExportID(asset);
			}
			foreach (IExportCollection collection in m_collections)
			{
				if (collection.IsContains(asset))
				{
					return collection.GetExportID(asset);
				}
			}

			if (Config.IsExportDependencies)
			{
				throw new InvalidOperationException($"Object {asset} wasn't found in any export collection");
			}
			else
			{
				return ExportCollection.GetMainExportID(asset);
			}
		}

		public AssetType ToExportType(ClassIDType classID)
		{
			return m_exporter.ToExportType(classID);
		}

		public ExportPointer CreateExportPointer(Object asset)
		{
			if (CurrentCollection.IsContains(asset))
			{
				return CurrentCollection.CreateExportPointer(asset, true);
			}
			foreach (IExportCollection collection in m_collections)
			{
				if (collection.IsContains(asset))
				{
					return collection.CreateExportPointer(asset, false);
				}
			}

			if (Config.IsExportDependencies)
			{
				//throw new InvalidOperationException($"Object {@object} wasn't found in any export collection");
			}
			ulong exportID = ExportCollection.GetMainExportID(asset);
			return new ExportPointer(exportID, EngineGUID.MissingReference, AssetType.Meta);
		}

		public string SceneIDToString(int sceneID)
		{
			return m_buildSettings == null ? $"level{sceneID}" : m_buildSettings.Scenes[sceneID];
		}

		public string TagIDToString(int tagID)
		{
			const string UntaggedTag = "Untagged";
			switch (tagID)
			{
				case 0:
					return UntaggedTag;
				case 1:
					return "Respawn";
				case 2:
					return "Finish";
				case 3:
					return "EditorOnly";
				//case 4:
				case 5:
					return "MainCamera";
				case 6:
					return "Player";
				case 7:
					return "GameController";
			}
			if(m_tagManager == null)
			{
				return UntaggedTag;
			}
			return m_tagManager.Tags[tagID - 20000];
		}

		public IExportCollection CurrentCollection { get; set; }
		public VirtualSerializedFile VirtualFile { get; } = new VirtualSerializedFile();
		public ISerializedFile File => CurrentCollection.File;
		public Version Version => File.Version;
		public Platform Platform => File.Platform;
		public TransferInstructionFlags Flags => File.Flags;
		
		private readonly ProjectExporter m_exporter;
		private readonly IReadOnlyList<IExportCollection> m_collections;

		private BuildSettings m_buildSettings;
		private TagManager m_tagManager;
	}
}
