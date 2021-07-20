using AssetRipper.Parser.Asset;
using AssetRipper.Parser.Classes.Object;
using AssetRipper.Parser.Classes.Utils.Extensions;

namespace AssetRipper.Parser.Utils.Extensions
{
	public static class IAssetContainerExtensions
	{
		public static string GetAssetLogString(this IAssetContainer _this, long pathID)
		{
			Object asset = _this.GetAsset(pathID);
			string name = asset.TryGetName();
			if (name == null)
			{
				return $"{asset.ClassID}_{pathID}";
			}
			else
			{
				return $"{asset.ClassID}_{pathID}({name})";
			}
		}
	}
}
