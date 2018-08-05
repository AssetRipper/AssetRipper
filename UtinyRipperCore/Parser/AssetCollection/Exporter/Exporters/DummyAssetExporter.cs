using System;
using System.Collections.Generic;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	internal class DummyAssetExporter : IAssetExporter
	{
		public bool IsHandle(Object asset)
		{
			return true;
		}

		public void Export(IExportContainer container, Object asset, string path)
		{
		}

		public void Export(IExportContainer container, Object asset, string path, Action<IExportContainer, Object, string> callback)
		{
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path)
		{
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path, Action<IExportContainer, Object, string> callback)
		{
		}

		public IExportCollection CreateCollection(Object asset)
		{
			switch(asset.ClassID)
			{
				case ClassIDType.MonoManager:
				case ClassIDType.AssetBundle:
				case ClassIDType.PreloadData:
					return new EmptyExportCollection();

				default:
					return new SkipExportCollection(this, asset);
			}
		}

		public AssetType ToExportType(Object asset)
		{
			ToUnknownExportType(asset.ClassID, out AssetType assetType);
			return assetType;
		}

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			switch (classID)
			{
				case ClassIDType.AnimatorController:
				case ClassIDType.MonoBehaviour:
					assetType = AssetType.Serialized;
					break;

				case ClassIDType.MonoScript:
				case ClassIDType.Sprite:
					assetType = AssetType.Meta;
					break;

				default:
					throw new NotSupportedException(classID.ToString());
			}
			return true;
		}
	}
}
