using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Core.Project.Collections
{
	public sealed class EmptyExportCollection : IExportCollection
	{
		public bool Export(IProjectAssetContainer container, string projectDirectory)
		{
			return false;
		}

		public bool IsContains(IUnityObjectBase asset)
		{
			return false;
		}

		public long GetExportID(IUnityObjectBase asset)
		{
			throw new NotSupportedException();
		}

		public UnityGUID GetExportGUID(IUnityObjectBase asset)
		{
			throw new NotSupportedException();
		}

		public MetaPtr CreateExportPointer(IUnityObjectBase asset, bool isLocal)
		{
			throw new NotSupportedException();
		}

		public ISerializedFile File => throw new NotSupportedException();
		public TransferInstructionFlags Flags => throw new NotSupportedException();
		public IEnumerable<IUnityObjectBase> Assets => Enumerable.Empty<IUnityObjectBase>();
		public string Name => throw new NotSupportedException();
	}
}
