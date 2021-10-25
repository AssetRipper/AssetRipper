using AssetRipper.Core.Classes;
using AssetRipper.Core.Interfaces;
using System;

namespace AssetRipper.Core.Extensions
{
	public static class ObjectExtensions
	{
		public static string GetOriginalName(this UnityObjectBase _this)
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

		public static string TryGetName(this UnityObjectBase _this)
		{
			if (_this is INamedObject named)
			{
				return named.ValidName;
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
