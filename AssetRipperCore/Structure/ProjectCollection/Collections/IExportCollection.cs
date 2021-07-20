using AssetRipper.Converters.Project;
using AssetRipper.Parser.Classes.Meta;
using AssetRipper.Parser.Classes.Object;
using AssetRipper.Parser.Files.SerializedFile;
using AssetRipper.Parser.IO.Asset;
using System.Collections.Generic;

namespace AssetRipper.Structure.ProjectCollection.Collections
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
