using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using AssetRipper.Core.Structure;
using System.Collections.Generic;

namespace AssetRipper.Core.Parser.Files.SerializedFiles
{
	public interface ISerializedFile : IAssetContainer
	{
		/// <summary>
		/// Try to find an asset in the current serialized file
		/// </summary>
		/// <param name="pathID">Path ID of the asset</param>
		/// <returns>Found asset or null</returns>
		IUnityObjectBase FindAsset(long pathID);

		ObjectInfo GetAssetEntry(long pathID);

		PPtr<T> CreatePPtr<T>(T asset) where T : IUnityObjectBase;

		IEnumerable<IUnityObjectBase> FetchAssets();

		IFileCollection Collection { get; }
	}
}
