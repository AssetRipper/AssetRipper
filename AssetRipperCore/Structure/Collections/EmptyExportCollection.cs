using AssetRipper.Project;
using AssetRipper.Classes.Meta;
using AssetRipper.Classes.Meta.Importers.Asset;
using AssetRipper.Classes.Misc;
using AssetRipper.Parser.Files.SerializedFiles;
using AssetRipper.IO.Asset;
using System;
using System.Collections.Generic;
using UnityObject = AssetRipper.Classes.Object.UnityObject;

namespace AssetRipper.Structure.Collections
{
	public sealed class EmptyExportCollection : IExportCollection
	{
		public bool Export(ProjectAssetContainer container, string dirPath)
		{
			return false;
		}

		public bool IsContains(UnityObject asset)
		{
			return false;
		}

		public long GetExportID(UnityObject asset)
		{
			throw new NotSupportedException();
		}

		public UnityGUID GetExportGUID(UnityObject asset)
		{
			throw new NotSupportedException();
		}

		public MetaPtr CreateExportPointer(UnityObject asset, bool isLocal)
		{
			throw new NotSupportedException();
		}

		public ISerializedFile File => throw new NotSupportedException();
		public TransferInstructionFlags Flags => throw new NotSupportedException();
		public IEnumerable<UnityObject> Assets
		{
			get { yield break; }
		}
		public string Name => throw new NotSupportedException();
		public AssetImporter MetaImporter => throw new NotSupportedException();
	}
}
