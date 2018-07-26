using System.Collections.Generic;
using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;

namespace UtinyRipper.AssetExporters
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

		public override ulong GetExportID(Object asset)
		{
			if (asset == Asset)
			{
				return base.GetExportID(asset);
			}
			return m_exportIDs[asset];
		}

		protected override string ExportInner(ProjectAssetContainer container, string filePath)
		{
			AssetExporter.Export(container, Assets, filePath);
			return filePath;
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

		protected virtual ulong GenerateExportID(Object asset)
		{
			return ObjectUtils.GenerateExportID(asset, IsContainsID);
		}

		protected void AddAsset(Object asset)
		{
			ulong exportID = GenerateExportID(asset);
			m_exportIDs.Add(asset, exportID);
		}

		private bool IsContainsID(ulong id)
		{
			return m_exportIDs.ContainsValue(id);
		}

		protected readonly Dictionary<Object, ulong> m_exportIDs = new Dictionary<Object, ulong>();
	}
}
