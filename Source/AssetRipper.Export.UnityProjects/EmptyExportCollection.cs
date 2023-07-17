using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Primitives;

namespace AssetRipper.Export.UnityProjects
{
	public sealed class EmptyExportCollection : IExportCollection
	{
		public static EmptyExportCollection Instance { get; } = new();

		private EmptyExportCollection()
		{
		}

		public bool Export(IExportContainer container, string projectDirectory)
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

		public UnityGuid GetExportGUID(IUnityObjectBase asset)
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
