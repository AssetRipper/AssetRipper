using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Utils;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Core.Project.Collections
{
	public abstract class AssetsExportCollection : AssetExportCollection
	{
		public AssetsExportCollection(IAssetExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset) { }

		public override bool IsContains(IUnityObjectBase asset)
		{
			if (base.IsContains(asset))
			{
				return true;
			}
			return m_exportIDs.ContainsKey(asset.AssetInfo);
		}

		public override long GetExportID(IUnityObjectBase asset)
		{
			if (asset.AssetInfo == Asset.AssetInfo)
			{
				return base.GetExportID(asset);
			}
			return m_exportIDs[asset.AssetInfo];
		}

		protected override bool ExportInner(IProjectAssetContainer container, string filePath, string dirPath)
		{
			return AssetExporter.Export(container, Assets.Select(t => Convert(t, container)), filePath);
		}

		public override IEnumerable<IUnityObjectBase> Assets
		{
			get
			{
				foreach (IUnityObjectBase asset in base.Assets)
				{
					yield return asset;
				}
				foreach (IUnityObjectBase asset in m_assets)
				{
					yield return asset;
				}
			}
		}

		protected virtual long GenerateExportID(IUnityObjectBase asset)
		{
			return ObjectUtils.GenerateExportID(asset, ContainsID);
		}

		protected void AddAsset(IUnityObjectBase asset)
		{
			long exportID = GenerateExportID(asset);
			m_assets.Add(asset);
			m_exportIDs.Add(asset.AssetInfo, exportID);
		}

		private bool ContainsID(long id)
		{
			return m_exportIDs.ContainsValue(id);
		}

		protected readonly List<IUnityObjectBase> m_assets = new();
		protected readonly Dictionary<AssetInfo, long> m_exportIDs = new();
	}
}
