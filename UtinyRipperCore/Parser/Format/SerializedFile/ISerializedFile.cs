using System.Collections.Generic;
using UtinyRipper.Classes;

namespace UtinyRipper.SerializedFiles
{
	public interface ISerializedFile
	{
		/// <summary>
		/// Get object in assets file with specified file index
		/// </summary>
		/// <param name="fileIndex">Dependency index</param>
		/// <param name="pathID">Path ID for searching object</param>
		/// <returns></returns>
		Object GetObject(int fileIndex, long pathID);
		/// <summary>
		/// Get object from current assets file
		/// </summary>
		/// <param name="fileIndex">Path ID for searching object</param>
		/// <returns>Founded object</returns>
		Object GetObject(long pathID);
		/// <summary>
		/// Try to find object in assets file with specified file index
		/// </summary>
		/// <param name="fileIndex">Dependency index</param>
		/// <param name="pathID">Path ID for searching object</param>
		/// <returns></returns>
		Object FindObject(int fileIndex, long pathID);
		/// <summary>
		/// Try to find object from current assets file
		/// </summary>
		/// <param name="pathID">Path ID for searching object</param>
		/// <returns>Founded object or null</returns>
		Object FindObject(long pathID);

		ObjectInfo GetObjectInfo(long pathID);
		ClassIDType GetClassID(long pathID);
		
		IEnumerable<Object> FetchAssets();

		string Name { get; }
		Platform Platform { get; }
		Version Version { get; }
		TransferInstructionFlags Flags { get; }

		bool IsScene { get; }

		IAssetCollection Collection { get; }
		IReadOnlyList<FileIdentifier> Dependencies { get; }
	}
}
