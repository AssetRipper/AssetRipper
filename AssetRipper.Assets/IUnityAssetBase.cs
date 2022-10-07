using AssetRipper.Assets.Export.Dependencies;
using AssetRipper.Assets.Interfaces;
using AssetRipper.Assets.IO;

namespace AssetRipper.Assets;

public interface IUnityAssetBase : IAsset, IDependent, ITypeTreeSerializable
{
}
