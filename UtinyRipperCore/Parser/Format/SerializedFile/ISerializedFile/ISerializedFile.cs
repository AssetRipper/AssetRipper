using System.Collections.Generic;
using UtinyRipper.Classes;

namespace UtinyRipper.SerializedFiles
{
	public interface ISerializedFile
	{
		/// <summary>
		/// Get object from current serialized file
		/// </summary>
		/// <param name="fileIndex">Path ID for searching object</param>
		/// <returns>Found object</returns>
		Object GetObject(long pathID);
		/// <summary>
		/// Get object in serialized file with specified file index
		/// </summary>
		/// <param name="fileIndex">Dependency index</param>
		/// <param name="pathID">Path ID for searching object</param>
		/// <returns>Found object</returns>
		Object GetObject(int fileIndex, long pathID);
		/// <summary>
		/// Try to find object from current assets file
		/// </summary>
		/// <param name="pathID">Path ID for searching object</param>
		/// <returns>Found object or null</returns>
		Object FindObject(long pathID);
		/// <summary>
		/// Try to find object in serialized file with specified file index
		/// </summary>
		/// <param name="fileIndex">Dependency index</param>
		/// <param name="pathID">Path ID for searching object</param>
		/// <returns>Found object or null</returns>
		Object FindObject(int fileIndex, long pathID);

		AssetEntry GetAssetEntry(long pathID);
		ClassIDType GetClassID(long pathID);
		
		IEnumerable<Object> FetchAssets();

		string Name { get; }
		Platform Platform { get; }
		Version Version { get; }
		TransferInstructionFlags Flags { get; }

		bool IsScene { get; }

		IFileCollection Collection { get; }
		IReadOnlyList<FileIdentifier> Dependencies { get; }
	}
}
