using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Classes.Meta.Importers.Asset;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Exporters;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Project.Collections
{
	public sealed class SkipExportCollection : IExportCollection
	{
		public SkipExportCollection(IAssetExporter assetExporter, IUnityObjectBase asset)
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
			m_asset = asset;
		}

		public bool Export(ProjectAssetContainer container, string dirPath)
		{
			return false;
		}

		public bool IsContains(IUnityObjectBase asset)
		{
			return asset == m_asset;
		}

		public long GetExportID(IUnityObjectBase asset)
		{
			if (asset == m_asset)
			{
				return ExportIdHandler.GetMainExportID(m_asset);
			}
			throw new ArgumentException(nameof(asset));
		}

		public UnityGUID GetExportGUID(IUnityObjectBase _)
		{
			throw new NotSupportedException();
		}

		public MetaPtr CreateExportPointer(IUnityObjectBase asset, bool isLocal)
		{
			if (isLocal)
			{
				throw new ArgumentException(nameof(isLocal));
			}

			long exportId = GetExportID(asset);
			AssetType type = AssetExporter.ToExportType(asset);
			return new MetaPtr(exportId, UnityGUID.MissingReference, type);
		}

		public IAssetExporter AssetExporter { get; }
		public ISerializedFile File => m_asset.SerializedFile;
		public TransferInstructionFlags Flags => File.Flags;
		public IEnumerable<IUnityObjectBase> Assets
		{
			get { yield return m_asset; }
		}
		public string Name => m_asset.GetType().Name;
		public IAssetImporter MetaImporter => throw new NotSupportedException();

		private readonly IUnityObjectBase m_asset;
	}
}
