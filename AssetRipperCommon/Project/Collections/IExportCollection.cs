using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using System.Collections.Generic;

namespace AssetRipper.Core.Project.Collections
{
	public interface IExportCollection
	{
		bool Export(IProjectAssetContainer container, string dirPath);
		bool IsContains(UnityObjectBase asset);
		long GetExportID(UnityObjectBase asset);
		MetaPtr CreateExportPointer(UnityObjectBase asset, bool isLocal);

		ISerializedFile File { get; }
		TransferInstructionFlags Flags { get; }
		IEnumerable<UnityObjectBase> Assets { get; }
		string Name { get; }
	}
}
