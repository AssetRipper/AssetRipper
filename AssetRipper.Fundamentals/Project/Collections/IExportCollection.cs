using AssetRipper.Core.Classes.Meta;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using System.Collections.Generic;

namespace AssetRipper.Core.Project.Collections
{
	public interface IExportCollection
	{
		/// <summary>
		/// Export the assets in this collection.
		/// </summary>
		/// <param name="container"></param>
		/// <param name="projectDirectory">The directory containing the whole project including Assets and ProjectSettings.</param>
		/// <returns>True if export was successful.</returns>
		bool Export(IProjectAssetContainer container, string projectDirectory);
		bool IsContains(IUnityObjectBase asset);
		long GetExportID(IUnityObjectBase asset);
		MetaPtr CreateExportPointer(IUnityObjectBase asset, bool isLocal);

		ISerializedFile File { get; }
		TransferInstructionFlags Flags { get; }
		IEnumerable<IUnityObjectBase> Assets { get; }
		string Name { get; }
	}
}
