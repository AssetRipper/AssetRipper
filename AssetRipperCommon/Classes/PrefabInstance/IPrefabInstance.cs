using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;

namespace AssetRipper.Core.Classes.PrefabInstance
{
	public interface IPrefabInstance : IAsset
	{
		string GetName(IAssetContainer file);
	}
}
