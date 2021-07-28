using AssetRipper.Parser.Asset;
using AssetRipper.Classes.Texture2D;
using AssetRipper.Parser.Files.SerializedFiles;
using AssetRipper.Structure.Collections;
using AssetRipper.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using Object = AssetRipper.Classes.Object.Object;

namespace AssetRipper.Project.Exporters
{
	public class BinaryAssetExporter : IAssetExporter
	{
		public virtual bool IsHandle(Object asset, ExportOptions options)
		{
			return true;
		}

		public bool Export(IExportContainer container, Object asset, string path)
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

		public void Export(IExportContainer container, Object asset, string path, Action<IExportContainer, Object, string> callback)
		{
			Export(container, asset, path);
			callback?.Invoke(container, asset, path);
		}

		public bool Export(IExportContainer container, IEnumerable<Object> assets, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path, Action<IExportContainer, Object, string> callback)
		{
			throw new NotSupportedException();
		}

		public virtual IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			switch (asset.ClassID)
			{
				case ClassIDType.Texture2D:
				case ClassIDType.Cubemap:
					return new TextureExportCollection(this, (Texture2D)asset, false);

				default:
					return new AssetExportCollection(this, asset);
			}
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
