using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;
using UtinyRipper.SerializedFiles;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	public sealed class EmptyExportCollection : IExportCollection
	{
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
		
		public ISerializedFile File => throw new NotSupportedException();
		public IEnumerable<Object> Objects
		{
			get { yield break; }
		}
		public string Name => throw new NotSupportedException();
		public IAssetImporter MetaImporter => throw new NotSupportedException();
	}
}
