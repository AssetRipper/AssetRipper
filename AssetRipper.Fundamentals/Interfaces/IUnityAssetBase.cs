using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;

namespace AssetRipper.Core.Interfaces
{
	public interface IUnityAssetBase : IAsset, IDependent, ITypeTreeSerializable
	{
	}
}
