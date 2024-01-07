using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.Import.AssetCreation;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.Utils;

namespace AssetRipper.Export.UnityProjects.RawAssets
{
	public sealed class UnreadableExportCollection : ExportCollection
	{
		UnreadableObject Asset { get; }
		public override IAssetExporter AssetExporter { get; }

		public UnreadableExportCollection(IAssetExporter exporter, UnreadableObject asset)
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

		public override string Name => Asset.Name;

		public override MetaPtr CreateExportPointer(IExportContainer container, IUnityObjectBase asset, bool isLocal)
		{
			return MetaPtr.NullPtr;
		}

		public override bool Export(IExportContainer container, string projectDirectory)
		{
			string name = DirectoryUtils.FixInvalidPathCharacters(Asset.Name);
			string resourcePath = Path.Combine(projectDirectory, "AssetRipper", "UnreadableAssets", Asset.ClassName, $"{name}.unreadable");
			string subPath = Path.GetDirectoryName(resourcePath)!;
			Directory.CreateDirectory(subPath);
			string resFileName = Path.GetFileName(resourcePath);
			string fileName = GetUniqueFileName(subPath, resFileName);
			string filePath = Path.Combine(subPath, fileName);
			return AssetExporter.Export(container, Asset, filePath);
		}

		public override long GetExportID(IExportContainer container, IUnityObjectBase asset)
		{
			throw new NotSupportedException();
		}

		public override bool Contains(IUnityObjectBase asset)
		{
			return asset == Asset;
		}
	}
}
