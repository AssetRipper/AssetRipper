using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;
using UtinyRipper.SerializedFiles;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	public class AssetsExportContainer : IAssetsExporter
	{
		public AssetsExportContainer(AssetsExporter exporter, List<IExportCollection> collections)
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

		public string GetExportID(Object @object)
		{
			if (CurrentCollection.IsContains(@object))
			{
				return CurrentCollection.GetExportID(@object);
			}
			foreach (IExportCollection collection in m_collections)
			{
				if (collection.IsContains(@object))
				{
					return collection.GetExportID(@object);
				}
			}

			if (Config.IsExportDependencies)
			{
				throw new InvalidOperationException($"Object {@object} wasn't found in any export collection");
			}
			else
			{
				return AssetExportCollection.GetMainExportID(@object);
			}
		}

		public AssetType ToExportType(ClassIDType classID)
		{
			return m_exporter.ToExportType(classID);
		}

		public ExportPointer CreateExportPointer(Object @object)
		{
			if (CurrentCollection.IsContains(@object))
			{
				return CurrentCollection.CreateExportPointer(@object, true);
			}
			foreach (IExportCollection collection in m_collections)
			{
				if (collection.IsContains(@object))
				{
					return collection.CreateExportPointer(@object, false);
				}
			}

			if (Config.IsExportDependencies)
			{
				throw new InvalidOperationException($"Object {@object} wasn't found in any export collection");
			}
			else
			{
				string exportID = AssetExportCollection.GetMainExportID(@object);
				return new ExportPointer(exportID, UtinyGUID.MissingReference, AssetType.Meta);
			}
		}

		public IExportCollection CurrentCollection { get; set; }
		public ISerializedFile File { get; set; }
		public Version Version => File.Version;
		public Platform Platform => File.Platform;
		
		private readonly AssetsExporter m_exporter;
		private readonly List<IExportCollection> m_collections;
	}
}
