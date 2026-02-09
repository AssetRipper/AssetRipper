using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Export.UnityProjects;

public interface IExportCollection
{
	/// <summary>
	/// Export the assets in this collection.
	/// </summary>
	/// <param name="container"></param>
	/// <param name="projectDirectory">The directory containing the whole project including Assets and ProjectSettings.</param>
	/// <returns>True if export was successful.</returns>
	bool Export(IExportContainer container, string projectDirectory, FileSystem fileSystem);
	/// <summary>
	/// Is the asset part of this collection?
	/// </summary>
	bool Contains(IUnityObjectBase asset);
	/// <summary>
	/// Get the export ID of the asset.
	/// </summary>
	long GetExportID(IExportContainer container, IUnityObjectBase asset);
	MetaPtr CreateExportPointer(IExportContainer container, IUnityObjectBase asset, bool isLocal);

	AssetCollection File { get; }
	TransferInstructionFlags Flags { get; }
	IEnumerable<IUnityObjectBase> Assets { get; }
	IEnumerable<IUnityObjectBase> ExportableAssets => Assets;
	/// <summary>
	/// Does this collection save any files?
	/// </summary>
	bool Exportable => true;
	string Name { get; }
}
