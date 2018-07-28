using System;
using System.IO;
using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	public class ManagerExportCollection : AssetExportCollection
	{
		public ManagerExportCollection(IAssetExporter assetExporter, Object asset):
			this(assetExporter, (GlobalGameManager)asset)
		{
		}

		public ManagerExportCollection(IAssetExporter assetExporter, GlobalGameManager asset) :
			base(assetExporter, asset)
		{
		}

		public override bool Export(ProjectAssetContainer container, string dirPath)
		{
			string subPath = Path.Combine(dirPath, "ProjectSettings");
			string fileName = $"{Asset.GetType().Name}.asset";
			string filePath = Path.Combine(subPath, fileName);

			if (!Directory.Exists(subPath))
			{
				Directory.CreateDirectory(subPath);
			}

			ExportInner(container, filePath);
			return true;
		}

		public override ulong GetExportID(Object asset)
		{
			if (asset == Asset)
			{
				return 1;
			}
			throw new ArgumentException(nameof(asset));
		}

		public override ExportPointer CreateExportPointer(Object asset, bool isLocal)
		{
			throw new NotSupportedException();
		}
	}
}
