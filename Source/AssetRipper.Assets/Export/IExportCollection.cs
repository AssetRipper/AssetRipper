using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Assets.Export
{
	public interface IExportCollection
	{
		/// <summary>
		/// Export the assets in this collection.
		/// </summary>
		/// <param name="container"></param>
		/// <param name="projectDirectory">The directory containing the whole project including Assets and ProjectSettings.</param>
		/// <returns>True if export was successful.</returns>
		bool Export(IExportContainer container, string projectDirectory);
		bool IsContains(IUnityObjectBase asset);
		long GetExportID(IUnityObjectBase asset);
		MetaPtr CreateExportPointer(IUnityObjectBase asset, bool isLocal);

		AssetCollection File { get; }
		TransferInstructionFlags Flags { get; }
		IEnumerable<IUnityObjectBase> Assets { get; }
		string Name { get; }
	}
}
