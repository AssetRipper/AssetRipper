using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Classes.Meta.Importers.Asset;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using System;
using System.Collections.Generic;
using Object = AssetRipper.Core.Classes.Object.Object;

namespace AssetRipper.Core.Project.Collections
{
	public sealed class EmptyExportCollection : IExportCollection
	{
		public bool Export(IProjectAssetContainer container, string dirPath)
		{
			return false;
		}

		public bool IsContains(UnityObjectBase asset)
		{
			return false;
		}

		public long GetExportID(UnityObjectBase asset)
		{
			throw new NotSupportedException();
		}

		public UnityGUID GetExportGUID(UnityObjectBase asset)
		{
			throw new NotSupportedException();
		}

		public MetaPtr CreateExportPointer(UnityObjectBase asset, bool isLocal)
		{
			throw new NotSupportedException();
		}

		public ISerializedFile File => throw new NotSupportedException();
		public TransferInstructionFlags Flags => throw new NotSupportedException();
		public IEnumerable<UnityObjectBase> Assets
		{
			get { yield break; }
		}
		public string Name => throw new NotSupportedException();
		public AssetImporter MetaImporter => throw new NotSupportedException();
	}
}
