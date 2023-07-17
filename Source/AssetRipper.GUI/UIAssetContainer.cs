using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects;
using AssetRipper.IO.Files.SerializedFiles;
using System.Collections.ObjectModel;

namespace AssetRipper.GUI
{
	public sealed class UIAssetContainer : ProjectAssetContainer
	{
		public UIAssetContainer(Ripper ripper) : base(
			new ProjectExporter(),
			ripper.GameStructure.FileCollection.AddNewTemporaryBundle().AddNew(),
			ripper.GameStructure.FileCollection.FetchAssets(),
			new Collection<IExportCollection>())
		{
		}

		public override IReadOnlyList<AssetCollection?> Dependencies => Array.Empty<AssetCollection?>();

		internal IUnityObjectBase? LastAccessedAsset { get; set; }

		public override AssetCollection File => LastAccessedAsset?.Collection ?? throw new NullReferenceException(nameof(LastAccessedAsset));

		public override TransferInstructionFlags ExportFlags => VirtualFile.Flags;
	}

}
