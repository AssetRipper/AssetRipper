using AssetRipper.Classes;

namespace AssetRipper
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
