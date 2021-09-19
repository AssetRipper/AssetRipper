using AssetRipper.Core.Configuration;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using Object = AssetRipper.Core.Classes.Object.Object;
using AssetRipper.Core.Project.Collections;

namespace AssetRipper.Core.Project.Exporters
{
	public class BinaryAssetExporter : IAssetExporter
	{
		public virtual bool IsHandle(Object asset, CoreConfiguration options)
		{
			return true;
		}

		public virtual bool Export(IExportContainer container, Object asset, string path)
		{
			using (Stream fileStream = FileUtils.CreateVirtualFile(path))
			{
				using (BufferedStream stream = new BufferedStream(fileStream))
				{
					asset.ExportBinary(container, stream);
				}
			}
			return true;
		}

		public virtual void Export(IExportContainer container, Object asset, string path, Action<IExportContainer, Object, string> callback)
		{
			Export(container, asset, path);
			callback?.Invoke(container, asset, path);
		}

		public virtual bool Export(IExportContainer container, IEnumerable<Object> assets, string path)
		{
			throw new NotSupportedException();
		}

		public virtual void Export(IExportContainer container, IEnumerable<Object> assets, string path, Action<IExportContainer, Object, string> callback)
		{
			throw new NotSupportedException();
		}

		public virtual IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new AssetExportCollection(this, asset);
		}

		public AssetType ToExportType(Object asset)
		{
			ToUnknownExportType(asset.ClassID, out AssetType assetType);
			return assetType;
		}

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}
	}
}
