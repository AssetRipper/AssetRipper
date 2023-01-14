using AssetRipper.Assets;
using AssetRipper.Assets.Interfaces;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class UnityObjectBaseExtensions
	{
		public static string GetOriginalName(this IUnityObjectBase _this)
		{
			if (_this is IHasNameString named)
			{
				return named.NameString;
			}
			else
			{
				throw new Exception($"Unable to get name for {_this.ClassID}");
			}
		}

		public static string? TryGetName(this IUnityObjectBase _this)
		{
			if (_this is IHasNameString named)
			{
				return named.NameString;
			}
			else
			{
				return null;
			}
		}
	}
}
