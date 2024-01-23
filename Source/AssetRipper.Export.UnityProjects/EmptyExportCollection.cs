using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Export.UnityProjects
{
	public sealed class EmptyExportCollection : IExportCollection
	{
		public static EmptyExportCollection Instance { get; } = new();

		private EmptyExportCollection()
		{
		}

		bool IExportCollection.Export(IExportContainer container, string projectDirectory)
		{
			throw new NotSupportedException();
		}

		bool IExportCollection.Contains(IUnityObjectBase asset)
		{
			return false;
		}

		long IExportCollection.GetExportID(IExportContainer container, IUnityObjectBase asset)
		{
			throw new NotSupportedException();
		}

		MetaPtr IExportCollection.CreateExportPointer(IExportContainer container, IUnityObjectBase asset, bool isLocal)
		{
			throw new NotSupportedException();
		}

		bool IExportCollection.Exportable => false;

		AssetCollection IExportCollection.File => throw new NotSupportedException();
		TransferInstructionFlags IExportCollection.Flags => throw new NotSupportedException();
		IEnumerable<IUnityObjectBase> IExportCollection.Assets => Enumerable.Empty<IUnityObjectBase>();
		public string Name => nameof(EmptyExportCollection);
	}
}
