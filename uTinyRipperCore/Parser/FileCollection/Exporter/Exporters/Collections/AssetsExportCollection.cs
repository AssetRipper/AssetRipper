using System.Collections.Generic;
using uTinyRipper.AssetExporters.Classes;
using uTinyRipper.Classes;

namespace uTinyRipper.AssetExporters
{
	public abstract class AssetsExportCollection : AssetExportCollection
	{
		public AssetsExportCollection(IAssetExporter assetExporter, Object asset) :
			this(assetExporter, asset, new NativeFormatImporter(asset))
		{
		}

		protected AssetsExportCollection(IAssetExporter assetExporter, Object asset, IAssetImporter metaImporter):
			base(assetExporter, asset, metaImporter)
		{
		}

		public override bool IsContains(Object asset)
		{
			if (base.IsContains(asset))
			{
				return true;
			}
			return m_exportIDs.ContainsKey(asset);
		}

		public override long GetExportID(Object asset)
		{
			if (asset == Asset)
			{
				return base.GetExportID(asset);
			}
			return m_exportIDs[asset];
		}

		protected override bool ExportInner(ProjectAssetContainer container, string filePath)
		{
			return AssetExporter.Export(container, Assets, filePath);
		}

		public override IEnumerable<Object> Assets
		{
			get
			{
				foreach (Object asset in base.Assets)
				{
					yield return asset;
				}
				foreach (Object asset in m_exportIDs.Keys)
				{
					yield return asset;
				}
			}
		}

		protected virtual long GenerateExportID(Object asset)
		{
			return ObjectUtils.GenerateExportID(asset, IsContainsID);
		}

		protected void AddAsset(Object asset)
		{
			long exportID = GenerateExportID(asset);
			m_exportIDs.Add(asset, exportID);
		}

		private bool IsContainsID(long id)
		{
			return m_exportIDs.ContainsValue(id);
		}

		protected readonly Dictionary<Object, long> m_exportIDs = new Dictionary<Object, long>();
	}
}
