using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Exporters;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Core.Project.Collections
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

		public override ISerializedFile File => Asset.SerializedFile;

		public override TransferInstructionFlags Flags => Asset.SerializedFile.Flags;

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
			string resourcePath = Path.Combine(projectDirectory, "AssetRipper", "UnreadableAssets", Asset.AssetClassName, $"{Asset.NameString}.unreadable");
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
