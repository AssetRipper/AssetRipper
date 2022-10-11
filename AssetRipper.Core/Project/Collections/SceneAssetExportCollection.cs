using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.Core.Classes;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SourceGenerated;
using System.Collections.Generic;

namespace AssetRipper.Core.Project.Collections
{
	public sealed class SceneAssetExportCollection : IExportCollection
	{
		public SceneAsset Asset { get; }

		public SceneAssetExportCollection(SceneAsset asset)
		{
			Asset = asset;
		}

		public AssetCollection File => Asset.Collection;
		public TransferInstructionFlags Flags => File.Flags;
		public IEnumerable<IUnityObjectBase> Assets
		{
			get
			{
				yield return Asset;
			}
		}

		public string Name => $"{Asset.TargetScene.Name} (SceneAsset)";

		public MetaPtr CreateExportPointer(IUnityObjectBase asset, bool isLocal)
		{
			return new MetaPtr(GetExportID(), Asset.TargetScene.GUID, AssetType.Meta);
		}

		private static long GetExportID()
		{
			return ExportIdHandler.GetMainExportID((uint)ClassIDType.DefaultAsset);
		}

		public bool Export(IProjectAssetContainer container, string projectDirectory)
		{
			return true;
		}

		public long GetExportID(IUnityObjectBase asset)
		{
			return GetExportID();
		}

		public bool IsContains(IUnityObjectBase asset)
		{
			return asset == Asset;
		}
	}
}
