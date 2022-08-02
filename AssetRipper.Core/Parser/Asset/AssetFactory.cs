using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Parser.Asset
{
	public class AssetFactory : AssetFactoryBase
	{
		public override IUnityObjectBase? CreateAsset(AssetInfo assetInfo, UnityVersion version)
		{
			return SourceGenerated.AssetFactory.CreateAsset(version, assetInfo);
		}

		public override ClassIDType GetClassIdForType(Type type)
		{
			if (SourceGenerated.ClassIDTypeMap.dictionary.TryGetValue(type, out SourceGenerated.ClassIDType value))
			{
				return (ClassIDType)value;
			}
			else
			{
				//throw new NotSupportedException(type.FullName);
				return ClassIDType.UnknownType;
			}
		}
	}
}
