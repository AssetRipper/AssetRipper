using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.Export.UnityProjects.Project.Exporters;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Export.UnityProjects.Project.Collections
{
	public class RawExportCollection : ExportCollection
	{
		IUnityObjectBase Asset { get; }
		public override IAssetExporter AssetExporter { get; }

		public RawExportCollection(IAssetExporter exporter, IUnityObjectBase asset)
		{
			Asset = asset;
			AssetExporter = exporter;
		}

		public override AssetCollection File => Asset.Collection;

		public override TransferInstructionFlags Flags => Asset.Collection.Flags;

		public override IEnumerable<IUnityObjectBase> Assets
		{
			get { yield return Asset; }
		}

		private string AssetTypeName => Asset.GetType().Name;

		public override string Name => $"RawObject_{AssetTypeName}";

		protected override string GetExportExtension(IUnityObjectBase asset) => "bytes";

		public override MetaPtr CreateExportPointer(IUnityObjectBase asset, bool isLocal)
		{
			throw new NotSupportedException();
		}

		public override bool Export(IExportContainer container, string projectDirectory)
		{
			string subPath = Path.Combine(projectDirectory, "AssetRipper", "RawData", AssetTypeName);
			Directory.CreateDirectory(subPath);
			string fileName = GetUniqueFileName(container.File, Asset, subPath);
			string filePath = Path.Combine(subPath, fileName);
			return AssetExporter.Export(container, Asset, filePath);
		}

		public override long GetExportID(IUnityObjectBase asset)
		{
			throw new NotSupportedException();
		}

		public override bool IsContains(IUnityObjectBase asset)
		{
			return asset == Asset;
		}
	}
}
