using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;

namespace AssetRipper.Core.Interfaces
{
	public interface IUnityAssetBase : IAsset, IDependent
	{
		UnityVersion AssetUnityVersion { get; set; }
		EndianType EndianType { get; set; }
		TransferInstructionFlags TransferInstructionFlags { get; set; }

		object TryGetFieldValue(string fieldName);
		T TryGetFieldValue<T>(string fieldName);
		bool TrySetFieldValue<T>(string fieldName, T value);
	}
}