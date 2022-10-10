using AssetRipper.Assets.Collections;
using System.Linq;
using AssetRipper.SourceGenerated.Classes.ClassID_3;

namespace AssetRipper.Core.Extensions
{
	public static class AssetCollectionExtensions
	{
		public static bool IsScene(this AssetCollection collection)
		{
			return collection.Any(asset => asset is ILevelGameManager);
		}
	}
}
