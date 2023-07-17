using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using System.Diagnostics;

namespace AssetRipper.Export.UnityProjects.Project
{
	public abstract class AssetsExportCollection<T> : AssetExportCollection<T> where T : IUnityObjectBase
	{
		public AssetsExportCollection(IAssetExporter assetExporter, T asset) : base(assetExporter, asset)
		{
			m_file = asset.Collection;
		}

		public override bool IsContains(IUnityObjectBase asset)
		{
			return base.IsContains(asset) || m_exportIDs.ContainsKey(asset);
		}

		public override long GetExportID(IUnityObjectBase asset)
		{
			if (asset.AssetInfo == Asset.AssetInfo)
			{
				return base.GetExportID(asset);
			}
			return m_exportIDs[asset];
		}

		protected override bool ExportInner(IExportContainer container, string filePath, string dirPath)
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
				foreach ((IUnityObjectBase asset, long _) in m_exportIDs)
				{
					m_file = asset.Collection;
					yield return asset;
				}
			}
		}

		protected virtual long GenerateExportID(IUnityObjectBase asset)
		{
			return ExportIdHandler.GetRandomExportId(asset, ContainsID);
		}

		/// <summary>
		/// Add an asset to this export collection.
		/// </summary>
		/// <param name="asset">The asset to be added to this export collection.</param>
		/// <returns><see langword="true"/> if the <paramref name="asset"/> is added to the <see cref="AssetsExportCollection"/> object; <see langword="false"/> if the <paramref name="asset"/> is already present.</returns>
		protected bool AddAsset(IUnityObjectBase asset)
		{
			Debug.Assert(asset != (IUnityObjectBase)Asset);
			long exportID = GenerateExportID(asset);
			if (m_exportIDs.TryAdd(asset, exportID))
			{
				m_exportAssetIds.Add(exportID);
				return true;
			}
			else
			{
				return false;
			}
		}

		private bool ContainsID(long id)
		{
			return m_exportAssetIds.Contains(id);
		}

		public override AssetCollection File => m_file;
		private AssetCollection m_file;

		/// <summary>
		/// A one-to-one dictionary of export id's
		/// </summary>
		private readonly Dictionary<IUnityObjectBase, long> m_exportIDs = new();

		private readonly HashSet<long> m_exportAssetIds = new();
	}
}
