using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Utils;
using System.Collections.Generic;
using System.Diagnostics;

namespace AssetRipper.Core.Project.Collections
{
	public abstract class AssetsExportCollection : AssetExportCollection
	{
		public AssetsExportCollection(IAssetExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
		{
			m_file = asset.Collection;
		}

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
			return AssetExporter.Export(container, Assets, filePath);
		}

		public override IEnumerable<IUnityObjectBase> Assets
		{
			get
			{
				foreach (IUnityObjectBase asset in base.Assets)
				{
					m_file = asset.Collection;
					yield return asset;
				}
				foreach (IUnityObjectBase asset in m_assets)
				{
					m_file = asset.Collection;
					yield return asset;
				}
			}
		}

		protected virtual long GenerateExportID(IUnityObjectBase asset)
		{
			return ObjectUtils.GenerateExportID(asset, ContainsID);
		}

		/// <summary>
		/// Add an asset to this export collection.
		/// </summary>
		/// <param name="asset">The asset to be added to this export collection.</param>
		/// <returns>True if the <paramref name="asset"/> was added or false if the <paramref name="asset"/> was already present.</returns>
		protected bool AddAsset(IUnityObjectBase asset)
		{
			Debug.Assert(asset != Asset);
			if (m_assets.Add(asset))
			{
				long exportID = GenerateExportID(asset);
				m_exportIDs.Add(asset.AssetInfo, exportID);
				m_exportAssetIds.Add(exportID);
				return true;
			}
			return false;
		}

		private bool ContainsID(long id)
		{
			return m_exportAssetIds.Contains(id);
			// return m_exportIDs.ContainsValue(id);
		}

		public override AssetCollection File => m_file;
		private AssetCollection m_file;

		protected readonly HashSet<IUnityObjectBase> m_assets = new();
		/// <summary>
		/// A one-to-one dictionary of export id's
		/// </summary>
		protected readonly Dictionary<AssetInfo, long> m_exportIDs = new();

		private readonly HashSet<long> m_exportAssetIds = new();
	}
}
