using AssetRipper.Classes;
using AssetRipper.SerializedFiles;
using System.Collections.Generic;

namespace AssetRipper
{
	public interface ISerializedFile : IAssetContainer
	{
		/// <summary>
		/// Try to find an asset in the current serialized file
		/// </summary>
		/// <param name="pathID">Path ID of the asset</param>
		/// <returns>Found asset or null</returns>
		Object FindAsset(long pathID);

		ObjectInfo GetAssetEntry(long pathID);

		PPtr<T> CreatePPtr<T>(T asset)
			where T : Object;

		IEnumerable<Object> FetchAssets();

		IFileCollection Collection { get; }
	}
}
