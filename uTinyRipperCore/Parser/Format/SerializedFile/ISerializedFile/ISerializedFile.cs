using System.Collections.Generic;
using uTinyRipper.Assembly;
using uTinyRipper.Classes;

namespace uTinyRipper.SerializedFiles
{
	public interface ISerializedFile : IAssetContainer
	{
		/// <summary>
		/// Get asset from current serialized file
		/// </summary>
		/// <param name="fileIndex">Path ID for searching object</param>
		/// <returns>Found object</returns>
		Object GetAsset(long pathID);
		/// <summary>
		/// Get asset in serialized file with specified file index
		/// </summary>
		/// <param name="fileIndex">Dependency index</param>
		/// <param name="pathID">Path ID for searching object</param>
		/// <returns>Found object</returns>
		Object GetAsset(int fileIndex, long pathID);
		/// <summary>
		/// Try to find asset from current assets file
		/// </summary>
		/// <param name="pathID">Path ID for searching object</param>
		/// <returns>Found object or null</returns>
		Object FindAsset(long pathID);

		AssetEntry GetAssetEntry(long pathID);
		ClassIDType GetClassID(long pathID);

		PPtr<T> CreatePPtr<T>(T asset)
			where T: Object;

		IEnumerable<Object> FetchAssets();

		string Name { get; }
		Platform Platform { get; }
		Version Version { get; }
		TransferInstructionFlags Flags { get; }

		IFileCollection Collection { get; }
		IAssemblyManager AssemblyManager { get; }
		IReadOnlyList<FileIdentifier> Dependencies { get; }
	}
}
