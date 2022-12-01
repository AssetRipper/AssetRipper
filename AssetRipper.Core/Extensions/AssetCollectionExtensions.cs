using AssetRipper.Assets.Collections;

namespace AssetRipper.Core.Extensions
{
	public static class AssetCollectionExtensions
	{
		public static bool IsScene(this AssetCollection collection)
		{
			return collection.IsScene;
		}
	}
}
