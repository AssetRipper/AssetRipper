using System;
using System.Collections.Generic;
using uTinyRipper.Project.Classes;
using uTinyRipper.Converters;

using Object = uTinyRipper.Classes.Object;
using uTinyRipper.Classes.Misc;

namespace uTinyRipper.Project
{
	public sealed class SkipExportCollection : IExportCollection
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
			m_asset = asset;
		}

		public bool Export(ProjectAssetContainer container, string dirPath)
		{
			return false;
		}

		public bool IsContains(Object asset)
		{
			return asset == m_asset;
		}

		public long GetExportID(Object asset)
		{
			if (asset == m_asset)
			{
				return ExportCollection.GetMainExportID(m_asset);
			}
			throw new ArgumentException(nameof(asset));
		}

		public GUID GetExportGUID(Object _)
		{
			throw new NotSupportedException();
		}

		public ExportPointer CreateExportPointer(Object asset, bool isLocal)
		{
			if (isLocal)
			{
				throw new ArgumentException(nameof(isLocal));
			}

			long exportId = GetExportID(asset);
			AssetType type = AssetExporter.ToExportType(asset);
			return new ExportPointer(exportId, GUID.MissingReference, type);
		}

		public IAssetExporter AssetExporter { get; }
		public ISerializedFile File => m_asset.File;
		public TransferInstructionFlags Flags => File.Flags;
		public IEnumerable<Object> Assets
		{
			get { yield return m_asset; }
		}
		public string Name => m_asset.GetType().Name;
		public IAssetImporter MetaImporter => throw new NotSupportedException();

		private readonly Object m_asset;
	}
}
