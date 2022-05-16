using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Parser.Asset
{
	public class AssetFactory : AssetFactoryBase
	{
		public override IUnityObjectBase? CreateAsset(AssetInfo assetInfo, UnityVersion version)
		{
			return SourceGenerated.AssetFactory.CreateAsset(version, assetInfo);
		}
	}
}
