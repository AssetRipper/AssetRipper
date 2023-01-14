using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Export.UnityProjects.Project.Collections
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

		public AssetCollection File => throw new NotSupportedException();
		public TransferInstructionFlags Flags => throw new NotSupportedException();
		public IEnumerable<IUnityObjectBase> Assets => Enumerable.Empty<IUnityObjectBase>();
		public string Name => throw new NotSupportedException();
	}
}
