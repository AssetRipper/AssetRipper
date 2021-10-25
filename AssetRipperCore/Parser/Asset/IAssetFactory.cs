using System;

namespace AssetRipper.Core.Parser.Asset
{
	public interface IAssetFactory
	{
		UnityObjectBase CreateAsset(AssetInfo assetInfo);
		void OverrideInstantiator(ClassIDType classType, Func<AssetInfo, UnityObjectBase> instantiator);
	}
}