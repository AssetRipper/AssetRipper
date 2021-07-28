using AssetRipper.Project;
using AssetRipper.Project.Exporters;
using AssetRipper.Parser.Asset;
using AssetRipper.Classes.Object;
using AssetRipper.Classes.Utils;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Structure.Collections
{
	public abstract class AssetsExportCollection : AssetExportCollection
	{
		public AssetsExportCollection(IAssetExporter assetExporter, UnityObject asset) : base(assetExporter, asset) { }

		public override bool IsContains(UnityObject asset)
		{
			if (base.IsContains(asset))
			{
				return true;
			}
			return m_exportIDs.ContainsKey(asset.AssetInfo);
		}

		public override long GetExportID(UnityObject asset)
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

		public override IEnumerable<UnityObject> Assets
		{
			get
			{
				foreach (UnityObject asset in base.Assets)
				{
					yield return asset;
				}
				foreach (UnityObject asset in m_assets)
				{
					yield return asset;
				}
			}
		}

		protected virtual long GenerateExportID(UnityObject asset)
		{
			return ObjectUtils.GenerateExportID(asset, ContainsID);
		}

		protected void AddAsset(UnityObject asset)
		{
			long exportID = GenerateExportID(asset);
			m_assets.Add(asset);
			m_exportIDs.Add(asset.AssetInfo, exportID);
		}

		private bool ContainsID(long id)
		{
			return m_exportIDs.ContainsValue(id);
		}

		protected readonly List<UnityObject> m_assets = new List<UnityObject>();
		protected readonly Dictionary<AssetInfo, long> m_exportIDs = new Dictionary<AssetInfo, long>();
	}
}
