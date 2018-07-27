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
		public ProjectAssetContainer(ProjectExporter exporter, List<IExportCollection> collections)
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
		}

		public Object FindObject(int fileIndex, long pathID)
		{
			if(fileIndex == PPtr<Object>.VirtualFileIndex)
			{
				return VirtualFile.FindObject(pathID);
			}
			else
			{
				return File.FindObject(fileIndex, pathID);
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

		public IExportCollection CurrentCollection { get; set; }
		public VirtualSerializedFile VirtualFile { get; } = new VirtualSerializedFile();
		public ISerializedFile File => CurrentCollection.File;
		public Version Version => File.Version;
		public Platform Platform => File.Platform;
		public TransferInstructionFlags Flags => File.Flags;

		private readonly ProjectExporter m_exporter;
		private readonly List<IExportCollection> m_collections;
	}
}
