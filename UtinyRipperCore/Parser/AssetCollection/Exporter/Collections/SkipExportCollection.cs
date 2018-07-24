using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;
using UtinyRipper.SerializedFiles;
using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	public class SkipExportCollection : IExportCollection
	{
		public SkipExportCollection(IAssetExporter assetExporter, Object asset)
		{
			if (assetExporter == null)
			{
				throw new ArgumentNullException(nameof(assetExporter));
			}
			if (asset == null)
			{
				throw new ArgumentNullException(nameof(asset));
			}

			AssetExporter = assetExporter;
			Name = asset.GetType().Name;
			m_asset = asset;
		}

		public bool Export(ProjectAssetContainer container, string dirPath)
		{
			return false;
		}

		public bool IsContains(Object @object)
		{
			return @object == m_asset;
		}

		public string GetExportID(Object @object)
		{
			if (@object == m_asset)
			{
				return $"{(int)m_asset.ClassID}00000";
			}
			throw new ArgumentException(nameof(@object));
		}

		public UtinyGUID GetExportGUID(Object asset)
		{
			throw new NotSupportedException();
		}

		public ExportPointer CreateExportPointer(Object @object, bool isLocal)
		{
			if (isLocal)
			{
				throw new ArgumentException(nameof(isLocal));
			}

			string exportId = GetExportID(@object);
			AssetType type = AssetExporter.ToExportType(@object.ClassID);
			return new ExportPointer(exportId, UtinyGUID.MissingReference, type);
		}

		public IAssetExporter AssetExporter { get; }
		public ISerializedFile File => m_asset.File;
		public IEnumerable<Object> Objects
		{
			get { yield return m_asset; }
		}
		public string Name { get; }
		public IAssetImporter MetaImporter => throw new NotSupportedException();

		private readonly Object m_asset;
	}
}
