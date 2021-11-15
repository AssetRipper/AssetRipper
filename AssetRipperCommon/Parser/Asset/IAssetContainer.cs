using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using System.Collections.Generic;

namespace AssetRipper.Core.Parser.Asset
{
	public interface IAssetContainer
	{
		/// <summary>
		/// Get asset from current asset container
		/// </summary>
		/// <param name="fileIndex">Path ID of the asset</param>
		/// <returns>Found asset</returns>
		IUnityObjectBase GetAsset(long pathID);
		/// <summary>
		/// Try to get asset in the dependency file with specified file index
		/// </summary>
		/// <param name="fileIndex">Dependent file index</param>
		/// <param name="pathID">Path ID of the asset</param>
		/// <returns>Found asset or null</returns>
		IUnityObjectBase FindAsset(int fileIndex, long pathID);
		/// <summary>
		/// Get asset in the dependency with specified file index
		/// </summary>
		/// <param name="fileIndex">Dependent file index</param>
		/// <param name="pathID">Path ID of the asset</param>
		/// <returns>Found asset</returns>
		IUnityObjectBase GetAsset(int fileIndex, long pathID);
		IUnityObjectBase FindAsset(ClassIDType classID);
		IUnityObjectBase FindAsset(ClassIDType classID, string name);

		ClassIDType GetAssetType(long pathID);

		string Name { get; }
		LayoutInfo Layout { get; }
		UnityVersion Version { get; }
		Platform Platform { get; }
		TransferInstructionFlags Flags { get; }

		IReadOnlyList<FileIdentifier> Dependencies { get; }
	}
}