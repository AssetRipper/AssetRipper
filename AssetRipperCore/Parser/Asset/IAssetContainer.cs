using AssetRipper.Layout;
using AssetRipper.Parser.Classes.Object;
using AssetRipper.Parser.Files;
using AssetRipper.Parser.Files.SerializedFiles.Parser;
using AssetRipper.IO.Asset;
using System.Collections.Generic;
using AssetRipper.Parser.Classes.Utils.Extensions;

namespace AssetRipper.Parser.Asset
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

	public static class IAssetContainerExtensions
	{
		public static string GetAssetLogString(this IAssetContainer _this, long pathID)
		{
			Object asset = _this.GetAsset(pathID);
			string name = asset.TryGetName();
			if (name == null)
			{
				return $"{asset.ClassID}_{pathID}";
			}
			else
			{
				return $"{asset.ClassID}_{pathID}({name})";
			}
		}
	}
}