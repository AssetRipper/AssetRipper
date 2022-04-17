using AssetRipper.Core.Classes;
using AssetRipper.Core.Interfaces;
using System;

namespace AssetRipper.Core.Extensions
{
	public static class ObjectExtensions
	{
		public static string GetOriginalName(this IUnityObjectBase _this)
		{
			if (_this is IHasName named)
			{
				return named.Name;
			}
			else
			{
				throw new Exception($"Unable to get name for {_this.ClassID}");
			}
		}

		public static string TryGetName(this IUnityObjectBase _this)
		{
			if (_this is INamedObject named)
			{
				return named.GetValidName();
			}
			else if (_this is IHasName named2)
			{
				return named2.Name;
			}
			else
			{
				return null;
			}
		}
	}
}
