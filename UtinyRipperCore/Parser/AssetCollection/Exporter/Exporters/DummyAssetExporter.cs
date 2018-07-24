using System;
using System.Collections.Generic;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	internal class DummyAssetExporter : IAssetExporter
	{
		public void Export(IExportContainer container, Object asset, string path)
		{
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path)
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

		public AssetType ToExportType(ClassIDType classID)
		{
			switch (classID)
			{
				case ClassIDType.AnimatorController:
					return AssetType.Serialized;
				case ClassIDType.MonoBehaviour:
					return AssetType.Serialized;

				case ClassIDType.MonoScript:
					return AssetType.Meta;
				case ClassIDType.Sprite:
					return AssetType.Meta;

				default:
					throw new NotSupportedException(classID.ToString());
			}
		}
	}
}
