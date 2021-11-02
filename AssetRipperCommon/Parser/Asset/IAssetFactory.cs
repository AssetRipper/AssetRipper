using AssetRipper.Core.Interfaces;
using System;

namespace AssetRipper.Core.Parser.Asset
{
	public interface IAssetFactory
	{
		IUnityObjectBase CreateAsset(AssetInfo assetInfo);
		void OverrideInstantiator(ClassIDType classType, Func<AssetInfo, IUnityObjectBase> instantiator);
	}
}