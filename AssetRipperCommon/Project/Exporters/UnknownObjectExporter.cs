using AssetRipper.Core.Classes;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Core.Project.Exporters
{
	public sealed class UnknownObjectExporter : IAssetExporter
	{
		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObjectBase asset)
		{
			return new UnknownExportCollection(this, (UnknownObject)asset);
		}

		public bool Export(IExportContainer container, UnityObjectBase asset, string path)
		{
			using (var fileStream = FileUtils.CreateVirtualFile(path))
			{
				using (BinaryWriter writer = new BinaryWriter(fileStream))
				{
					writer.Write(((UnknownObject)asset).Data);
				}
			}
			return true;
		}

		public void Export(IExportContainer container, UnityObjectBase asset, string path, Action<IExportContainer, UnityObjectBase, string> callback)
		{
			if(Export(container, asset, path))
				callback?.Invoke(container, asset, path);
		}

		public bool Export(IExportContainer container, IEnumerable<UnityObjectBase> assets, string path)
		{
			bool success = true;
			foreach(var asset in assets)
			{
				success &= Export(container, asset, path);
			}
			return success;
		}

		public void Export(IExportContainer container, IEnumerable<UnityObjectBase> assets, string path, Action<IExportContainer, UnityObjectBase, string> callback)
		{
			foreach (var asset in assets)
			{
				Export(container, asset, path, callback);
			}
		}

		public bool IsHandle(UnityObjectBase asset)
		{
			return asset is UnknownObject;
		}

		public AssetType ToExportType(UnityObjectBase asset)
		{
			return AssetType.Meta;
		}

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}
	}
}
