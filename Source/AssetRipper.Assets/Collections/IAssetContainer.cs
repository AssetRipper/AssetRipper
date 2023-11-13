using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Assets.Collections
{
	public interface IAssetContainer
	{
		/// <summary>
		/// Try to find an asset in the current serialized file
		/// </summary>
		/// <param name="pathID">Path ID of the asset</param>
		/// <returns>Found asset or null</returns>
		IUnityObjectBase? TryGetAsset(long pathID);
		/// <summary>
		/// Try to get asset in the dependency file with specified file index
		/// </summary>
		/// <param name="fileIndex">Dependent file index</param>
		/// <param name="pathID">Path ID of the asset</param>
		/// <returns>Found asset or null</returns>
		IUnityObjectBase? TryGetAsset(int fileIndex, long pathID);

		string Name { get; }
		UnityVersion Version { get; }
		BuildTarget Platform { get; }
		TransferInstructionFlags Flags { get; }

		IReadOnlyList<AssetCollection?> Dependencies { get; }
	}
}
