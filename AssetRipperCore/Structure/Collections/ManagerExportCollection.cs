using AssetRipper.Project;
using AssetRipper.Project.Exporters;
using AssetRipper.Classes;
using AssetRipper.Classes.Meta;
using AssetRipper.Utils;
using System;
using System.IO;
using Object = AssetRipper.Classes.Object.Object;

namespace AssetRipper.Structure.Collections
{
	public class ManagerExportCollection : AssetExportCollection
	{
		public ManagerExportCollection(IAssetExporter assetExporter, Object asset) : this(assetExporter, (GlobalGameManager)asset) { }

		public ManagerExportCollection(IAssetExporter assetExporter, GlobalGameManager asset) : base(assetExporter, asset) { }

		public override bool Export(ProjectAssetContainer container, string dirPath)
		{
			string subPath = Path.Combine(dirPath, ProjectSettingsName);
			string fileName = $"{Asset.ExportPath}.asset";
			string filePath = Path.Combine(subPath, fileName);

			if (!DirectoryUtils.Exists(subPath))
			{
				DirectoryUtils.CreateVirtualDirectory(subPath);
			}

			ExportInner(container, filePath);
			return true;
		}

		public override long GetExportID(Object asset)
		{
			if (asset == Asset)
			{
				return 1;
			}
			throw new ArgumentException(nameof(asset));
		}

		public override MetaPtr CreateExportPointer(Object asset, bool isLocal)
		{
			throw new NotSupportedException();
		}

		protected const string ProjectSettingsName = "ProjectSettings";
	}
}
