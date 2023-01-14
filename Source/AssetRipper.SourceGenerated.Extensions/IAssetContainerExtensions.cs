using AssetRipper.Assets;
using AssetRipper.Assets.Collections;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class IAssetContainerExtensions
	{
		public static string GetAssetLogString(this IAssetContainer _this, long pathID)
		{
			IUnityObjectBase asset = _this.GetAsset(pathID);
			string? name = asset.TryGetName();
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
