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
		bool IsContains(Object asset);
		long GetExportID(Object asset);
		MetaPtr CreateExportPointer(Object asset, bool isLocal);

		ISerializedFile File { get; }
		TransferInstructionFlags Flags { get; }
		IEnumerable<Object> Assets { get; }
		string Name { get; }
	}
}
