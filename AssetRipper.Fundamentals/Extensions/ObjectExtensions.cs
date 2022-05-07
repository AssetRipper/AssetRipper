using AssetRipper.Core.Classes;
using AssetRipper.Core.Interfaces;
using System;

namespace AssetRipper.Core.Extensions
{
	public static class ObjectExtensions
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
