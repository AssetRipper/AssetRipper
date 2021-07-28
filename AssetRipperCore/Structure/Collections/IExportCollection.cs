using AssetRipper.Project;
using AssetRipper.Classes.Meta;
using AssetRipper.Classes.Object;
using AssetRipper.Parser.Files.SerializedFiles;
using AssetRipper.IO.Asset;
using System.Collections.Generic;

namespace AssetRipper.Structure.Collections
{
	public interface IExportCollection
	{
		bool Export(ProjectAssetContainer container, string dirPath);
		bool IsContains(UnityObject asset);
		long GetExportID(UnityObject asset);
		MetaPtr CreateExportPointer(UnityObject asset, bool isLocal);

		ISerializedFile File { get; }
		TransferInstructionFlags Flags { get; }
		IEnumerable<UnityObject> Assets { get; }
		string Name { get; }
	}
}
