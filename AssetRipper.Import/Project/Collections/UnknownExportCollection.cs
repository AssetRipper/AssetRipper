using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.Import.Classes;
using AssetRipper.Import.Project.Exporters;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Import.Project.Collections
{
	public sealed class UnknownExportCollection : ExportCollection
	{
		UnknownObject Asset { get; }
		public override IAssetExporter AssetExporter { get; }

		public UnknownExportCollection(IAssetExporter exporter, UnknownObject asset)
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

		public override string Name => Asset.NameString;

		public override MetaPtr CreateExportPointer(IUnityObjectBase asset, bool isLocal)
		{
			return MetaPtr.NullPtr;
		}

		public override bool Export(IProjectAssetContainer container, string projectDirectory)
		{
			string resourcePath = Path.Combine(projectDirectory, "AssetRipper", "UnknownAssets", Asset.ClassName, $"{Asset.NameString}.unknown");
			string subPath = Path.GetDirectoryName(resourcePath)!;
			Directory.CreateDirectory(subPath);
			string resFileName = Path.GetFileName(resourcePath);
			string fileName = GetUniqueFileName(subPath, resFileName);
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
