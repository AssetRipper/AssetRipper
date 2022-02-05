using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using System.Collections.Generic;

namespace AssetRipper.Core.Project.Collections
{
	public interface IExportCollection
	{
		bool Export(ProjectAssetContainer container, string dirPath);
		bool IsContains(IUnityObjectBase asset);
		long GetExportID(IUnityObjectBase asset);
		MetaPtr CreateExportPointer(IUnityObjectBase asset, bool isLocal);

		ISerializedFile File { get; }
		TransferInstructionFlags Flags { get; }
		IEnumerable<IUnityObjectBase> Assets { get; }
		string Name { get; }
	}
}
