using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	internal sealed class EmptyExportCollection : IExportCollection
	{
		public EmptyExportCollection(IAssetExporter assetExporter)
		{
			if (assetExporter == null)
			{
				throw new ArgumentNullException(nameof(assetExporter));
			}
			AssetExporter = assetExporter;
			Name = string.Empty;
		}

		public bool Export(ProjectAssetContainer container, string dirPath)
		{
			return false;
		}

		public bool IsContains(Object @object)
		{
			return false;
		}

		public string GetExportID(Object @object)
		{
			throw new NotSupportedException();
		}

		public UtinyGUID GetExportGUID(Object asset)
		{
			throw new NotSupportedException();
		}

		public ExportPointer CreateExportPointer(Object @object, bool isLocal)
		{
			throw new NotSupportedException();
		}

		public IAssetExporter AssetExporter { get; }
		public IEnumerable<Object> Objects
		{
			get { yield break; }
		}
		public string Name { get; }
		public IAssetImporter MetaImporter => throw new NotSupportedException();
	}
}
