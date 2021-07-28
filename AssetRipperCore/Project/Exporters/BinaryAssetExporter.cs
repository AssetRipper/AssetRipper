using AssetRipper.Parser.Asset;
using AssetRipper.Classes.Texture2D;
using AssetRipper.Parser.Files.SerializedFiles;
using AssetRipper.Structure.Collections;
using AssetRipper.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using UnityObject = AssetRipper.Classes.Object.UnityObject;

namespace AssetRipper.Project.Exporters
{
	public class BinaryAssetExporter : IAssetExporter
	{
		public virtual bool IsHandle(UnityObject asset, ExportOptions options)
		{
			return true;
		}

		public bool Export(IExportContainer container, UnityObject asset, string path)
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

		public void Export(IExportContainer container, UnityObject asset, string path, Action<IExportContainer, UnityObject, string> callback)
		{
			Export(container, asset, path);
			callback?.Invoke(container, asset, path);
		}

		public bool Export(IExportContainer container, IEnumerable<UnityObject> assets, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, IEnumerable<UnityObject> assets, string path, Action<IExportContainer, UnityObject, string> callback)
		{
			throw new NotSupportedException();
		}

		public virtual IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObject asset)
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

		public AssetType ToExportType(UnityObject asset)
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
