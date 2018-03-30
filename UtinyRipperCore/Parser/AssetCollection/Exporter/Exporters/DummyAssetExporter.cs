using System;
using UtinyRipper.Classes;
using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	internal class DummyAssetExporter : IAssetExporter
	{
		public IExportCollection CreateCollection(UtinyRipper.Classes.Object @object)
		{
			if(@object is MonoScript monoScript)
			{
				return new SkipExportCollection(this, monoScript, monoScript.ClassName);
			}
			if(@object is BuildSettings buildSettings)
			{
				return new SkipExportCollection(this, buildSettings, typeof(BuildSettings).Name);
			}
			if(@object is AssetBundle bundle)
			{
				string name = AssetBundle.IsReadAssetBundleName(bundle.File.Version) ? bundle.AssetBundleName : bundle.Name;
				return new EmptyExportCollection(this, name);
			}
			return new SkipExportCollection(this, (NamedObject)@object);
		}

		public bool Export(IAssetsExporter exporter, IExportCollection collection, string dirPath)
		{
			return false;
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
