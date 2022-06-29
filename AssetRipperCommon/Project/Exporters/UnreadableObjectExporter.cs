using AssetRipper.Core.Classes;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Core.Project.Exporters
{
	internal class UnreadableObjectExporter : IAssetExporter
	{
		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new UnreadableExportCollection(this, (UnreadableObject)asset);
		}

		public bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			using FileStream fileStream = File.Create(path);
			using BinaryWriter writer = new BinaryWriter(fileStream);
			writer.Write(((UnreadableObject)asset).RawData);
			return true;
		}

		public void Export(IExportContainer container, IUnityObjectBase asset, string path, Action<IExportContainer, IUnityObjectBase, string> callback)
		{
			if (Export(container, asset, path))
			{
				callback?.Invoke(container, asset, path);
			}
		}

		public bool Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path)
		{
			bool success = true;
			foreach (IUnityObjectBase? asset in assets)
			{
				success &= Export(container, asset, path);
			}
			return success;
		}

		public void Export(IExportContainer container, IEnumerable<IUnityObjectBase> assets, string path, Action<IExportContainer, IUnityObjectBase, string> callback)
		{
			foreach (IUnityObjectBase asset in assets)
			{
				Export(container, asset, path, callback);
			}
		}

		public bool IsHandle(IUnityObjectBase asset)
		{
			return asset is UnreadableObject;
		}

		public AssetType ToExportType(IUnityObjectBase asset)
		{
			return AssetType.Meta;
		}

		public bool ToUnknownExportType(Type type, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}
	}
}
