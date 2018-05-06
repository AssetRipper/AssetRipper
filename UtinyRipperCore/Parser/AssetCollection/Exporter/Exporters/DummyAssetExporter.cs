using System;
using System.Collections.Generic;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	internal class DummyAssetExporter : IAssetExporter
	{
		public void Export(ProjectAssetContainer container, Object asset, string path)
		{
		}

		public void Export(ProjectAssetContainer container, IEnumerable<Object> assets, string path)
		{
		}

		public IExportCollection CreateCollection(Object asset)
		{
			switch(asset.ClassID)
			{
				case ClassIDType.AssetBundle:
					return new EmptyExportCollection(this);

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
