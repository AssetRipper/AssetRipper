using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Utils;
using System;
using System.IO;

namespace AssetRipper.Core.Project.Collections
{
	public class ManagerExportCollection : AssetExportCollection
	{
		public ManagerExportCollection(IAssetExporter assetExporter, IUnityObjectBase asset) : this(assetExporter, (IGlobalGameManager)asset) { }

		public ManagerExportCollection(IAssetExporter assetExporter, IGlobalGameManager asset) : base(assetExporter, asset) { }

		public override bool Export(IProjectAssetContainer container, string dirPath)
		{
			string subPath = Path.Combine(dirPath, ProjectSettingsName);
			string fileName = $"{Asset.ExportPath}.asset";
			string filePath = Path.Combine(subPath, fileName);

			if (!Directory.Exists(subPath))
			{
				DirectoryUtils.CreateVirtualDirectory(subPath);
			}

			ExportInner(container, filePath);
			return true;
		}

		public override long GetExportID(IUnityObjectBase asset)
		{
			if (asset == Asset)
			{
				return 1;
			}
			throw new ArgumentException(nameof(asset));
		}

		public override MetaPtr CreateExportPointer(IUnityObjectBase asset, bool isLocal)
		{
			throw new NotSupportedException();
		}

		protected const string ProjectSettingsName = "ProjectSettings";
	}
}
