using System;
using System.Collections.Generic;
using AssetRipper.Classes;
using AssetRipper.Classes.Misc;
using AssetRipper.Converters;

using Object = AssetRipper.Classes.Object;

namespace AssetRipper.Project
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

		public UnityGUID GetExportGUID(Object asset)
		{
			throw new NotSupportedException();
		}

		public MetaPtr CreateExportPointer(Object asset, bool isLocal)
		{
			throw new NotSupportedException();
		}
		
		public ISerializedFile File => throw new NotSupportedException();
		public TransferInstructionFlags Flags => throw new NotSupportedException();
		public IEnumerable<Object> Assets
		{
			get { yield break; }
		}
		public string Name => throw new NotSupportedException();
		public AssetImporter MetaImporter => throw new NotSupportedException();
	}
}
