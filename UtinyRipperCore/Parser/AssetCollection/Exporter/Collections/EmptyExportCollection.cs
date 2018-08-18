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

		public bool IsContains(Object asset)
		{
			return false;
		}

		public long GetExportID(Object asset)
		{
			throw new NotSupportedException();
		}

		public EngineGUID GetExportGUID(Object asset)
		{
			throw new NotSupportedException();
		}

		public ExportPointer CreateExportPointer(Object asset, bool isLocal)
		{
			throw new NotSupportedException();
		}
		
		public ISerializedFile File => throw new NotSupportedException();
		public IEnumerable<Object> Assets
		{
			get { yield break; }
		}
		public string Name => throw new NotSupportedException();
		public IAssetImporter MetaImporter => throw new NotSupportedException();
	}
}
