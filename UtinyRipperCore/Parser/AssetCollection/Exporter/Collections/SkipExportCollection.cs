using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	internal class SkipExportCollection : IExportCollection
	{
		public SkipExportCollection(DummyAssetExporter assetExporter, Object asset):
			this(assetExporter, asset, asset.GetType().Name)
		{
		}

		public SkipExportCollection(DummyAssetExporter assetExporter, NamedObject asset):
			this(assetExporter, asset, asset.Name)
		{
		}

		public SkipExportCollection(DummyAssetExporter assetExporter, Object asset, string name)
		{
			if (assetExporter == null)
			{
				throw new ArgumentNullException(nameof(assetExporter));
			}
			if (asset == null)
			{
				throw new ArgumentNullException(nameof(asset));
			}
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			AssetExporter = assetExporter;
			Name = name;
			m_asset = asset;
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
		public IEnumerable<Object> Objects
		{
			get { yield return m_asset; }
		}
		public string Name { get; }
		public UtinyGUID GUID => throw new NotSupportedException();
		public IYAMLExportable MetaImporter => throw new NotSupportedException();

		private readonly Object m_asset;
	}
}
