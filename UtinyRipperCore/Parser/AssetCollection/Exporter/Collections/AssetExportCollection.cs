using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	public class AssetExportCollection : IExportCollection
	{
		public AssetExportCollection(IAssetExporter assetExporter, Object asset)
		{
			if (assetExporter == null)
			{
				throw new ArgumentNullException(nameof(assetExporter));
			}
			if (asset == null)
			{
				throw new ArgumentNullException(nameof(asset));
			}
			AssetExporter = assetExporter;
			Asset = asset;
			MetaImporter = CreateImporter(asset);
		}

		public static string GetMainExportID(Object @object)
		{
			return $"{(int)@object.ClassID}00000";
		}

		public virtual bool IsContains(Object @object)
		{
			return Asset == @object;
		}

		public virtual string GetExportID(Object @object)
		{
			if(@object == Asset)
			{
				return GetMainExportID(Asset);
			}
			throw new ArgumentException(nameof(@object));
		}

		public ExportPointer CreateExportPointer(Object @object, bool isLocal)
		{
			string exportID = GetExportID(@object);
			return isLocal ?
				new ExportPointer(exportID) :
				new ExportPointer(exportID, Asset.GUID, AssetExporter.ToExportType(Asset.ClassID));
		}

		protected virtual IYAMLExportable CreateImporter(Object asset)
		{
			return new NativeFormatImporter(asset);
		}

		public IAssetExporter AssetExporter { get; }
		public virtual IEnumerable<Object> Objects
		{
			get { yield return Asset; }
		}
		public string Name => Asset.ToString();
		public UtinyGUID GUID => Asset.GUID;
		public Object Asset { get; }
		public IYAMLExportable MetaImporter { get; }
	}
}
