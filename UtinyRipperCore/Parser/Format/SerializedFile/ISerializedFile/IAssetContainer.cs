using UtinyRipper.Classes;

namespace UtinyRipper.SerializedFiles
{
	public interface IAssetContainer
	{
		/// <summary>
		/// Try to find asset in serialized file with specified file index
		/// </summary>
		/// <param name="fileIndex">Dependency index</param>
		/// <param name="pathID">Path ID for searching object</param>
		/// <returns>Found object or null</returns>
		Object FindAsset(int fileIndex, long pathID);
		Object FindAsset(ClassIDType classID);
		Object FindAsset(ClassIDType classID, string name);
	}
}
