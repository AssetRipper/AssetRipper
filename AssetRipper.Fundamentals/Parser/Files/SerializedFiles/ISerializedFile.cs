using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using AssetRipper.Core.Structure;
using AssetRipper.IO.Endian;
using System.Collections.Generic;

namespace AssetRipper.Core.Parser.Files.SerializedFiles
{
	public interface ISerializedFile : IAssetContainer
	{
		ObjectInfo GetAssetEntry(long pathID);

		PPtr<T> CreatePPtr<T>(T asset) where T : IUnityObjectBase;

		IEnumerable<IUnityObjectBase> FetchAssets();

		IFileCollection Collection { get; }
		EndianType EndianType { get; }
	}
}
