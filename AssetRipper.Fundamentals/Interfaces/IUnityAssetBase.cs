using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Parser.Asset;

namespace AssetRipper.Core.Interfaces
{
	public interface IUnityAssetBase : IAsset, IDependent, ITypeTreeSerializable
	{
		UnityVersion AssetUnityVersion { get; set; }
		EndianType EndianType { get; set; }
		TransferInstructionFlags TransferInstructionFlags { get; set; }
	}
}