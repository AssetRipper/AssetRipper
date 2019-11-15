using System.Collections.Generic;
using uTinyRipper.Classes;
using uTinyRipper.Layout;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper
{
	public interface IAssetContainer
	{
		/// <summary>
		/// Get asset from current asset container
		/// </summary>
		/// <param name="fileIndex">Path ID of the asset</param>
		/// <returns>Found asset</returns>
		Object GetAsset(long pathID);
		/// <summary>
		/// Try to get asset in the dependency file with specified file index
		/// </summary>
		/// <param name="fileIndex">Dependent file index</param>
		/// <param name="pathID">Path ID of the asset</param>
		/// <returns>Found asset or null</returns>
		Object FindAsset(int fileIndex, long pathID);
		/// <summary>
		/// Get asset in the dependency with specified file index
		/// </summary>
		/// <param name="fileIndex">Dependent file index</param>
		/// <param name="pathID">Path ID of the asset</param>
		/// <returns>Found asset</returns>
		Object GetAsset(int fileIndex, long pathID);
		Object FindAsset(ClassIDType classID);
		Object FindAsset(ClassIDType classID, string name);

		ClassIDType GetAssetType(long pathID);

		string Name { get; }
		AssetLayout Layout { get; }
		Version Version { get; }
		Platform Platform { get; }
		TransferInstructionFlags Flags { get; }

		IReadOnlyList<FileIdentifier> Dependencies { get; }
	}
}