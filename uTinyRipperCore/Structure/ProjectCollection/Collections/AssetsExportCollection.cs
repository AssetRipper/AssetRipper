using System.Collections.Generic;
using System.Linq;
using uTinyRipper.Classes;
using uTinyRipper.Converters;

namespace uTinyRipper.Project
{
	public abstract class AssetsExportCollection : AssetExportCollection
	{
		public AssetsExportCollection(IAssetExporter assetExporter, Object asset) :
			base(assetExporter, asset)
		{
		}

		public override bool IsContains(Object asset)
		{
			if (base.IsContains(asset))
			{
				return true;
			}
			return m_exportIDs.ContainsKey(asset.AssetInfo);
		}

		public override long GetExportID(Object asset)
		{
			if (asset.AssetInfo == Asset.AssetInfo)
			{
				return base.GetExportID(asset);
			}
			return m_exportIDs[asset.AssetInfo];
		}

		protected override bool ExportInner(ProjectAssetContainer container, string filePath)
		{
			return AssetExporter.Export(container, Assets.Select(t => t.Convert(container)), filePath);
		}

		public override IEnumerable<Object> Assets
		{
			get
			{
				foreach (Object asset in base.Assets)
				{
					yield return asset;
				}
				foreach (Object asset in m_assets)
				{
					yield return asset;
				}
			}
		}

		protected virtual long GenerateExportID(Object asset)
		{
			return ObjectUtils.GenerateExportID(asset, ContainsID);
		}

		protected void AddAsset(Object asset)
		{
			long exportID = GenerateExportID(asset);
			m_assets.Add(asset);
			m_exportIDs.Add(asset.AssetInfo, exportID);
		}

		private bool ContainsID(long id)
		{
			return m_exportIDs.ContainsValue(id);
		}

		protected readonly List<Object> m_assets = new List<Object>();
		protected readonly Dictionary<AssetInfo, long> m_exportIDs = new Dictionary<AssetInfo, long>();
	}
}
