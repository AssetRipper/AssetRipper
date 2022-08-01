using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Library;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AssetRipper.GUI
{
	public class UIAssetContainer : ProjectAssetContainer
	{

		public UIAssetContainer(Ripper ripper) : base(
			ripper.GameStructure.Exporter,
			ripper.Settings,
			new VirtualSerializedFile(ripper.GameStructure.FileCollection.Layout),
			ripper.GameStructure.FileCollection.FetchAssets(),
			new Collection<IExportCollection>())
		{
		}
		public override IReadOnlyList<FileIdentifier> Dependencies => new List<FileIdentifier>();

		internal IUnityObjectBase LastAccessedAsset { get; set; }

		public override ISerializedFile File => LastAccessedAsset.SerializedFile;

		public override TransferInstructionFlags ExportFlags => ExportLayout.Flags;
	}

}