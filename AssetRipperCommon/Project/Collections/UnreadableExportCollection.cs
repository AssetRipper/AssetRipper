using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Exporters;
using System;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Core.Project.Collections
{
	public class UnreadableExportCollection : ExportCollection
	{
		UnreadableObject Asset { get; }
		public override IAssetExporter AssetExporter { get; }

		public UnreadableExportCollection(IAssetExporter exporter, UnreadableObject asset)
		{
			Asset = asset;
			AssetExporter = exporter;
		}

		public override ISerializedFile File => Asset.SerializedFile;

		public override TransferInstructionFlags Flags => Asset.TransferInstructionFlags;

		public override IEnumerable<IUnityObjectBase> Assets
		{
			get { yield return Asset; }
		}

		public override string Name => string.IsNullOrWhiteSpace(Asset.Name) ? $"UnreadableObject_{Asset.ClassID}" : $"UnreadableObject_{Asset.Name}";

		public override MetaPtr CreateExportPointer(IUnityObjectBase asset, bool isLocal)
		{
			throw new NotSupportedException();
		}

		public override bool Export(ProjectAssetContainer container, string dirPath)
		{
			string subFolder = Asset.ExportPath;
			string subPath = Path.Combine(dirPath, subFolder);
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
