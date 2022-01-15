using AssetRipper.Core.Interfaces;
using System;

namespace AssetRipper.Core.Extensions
{
	public static class TypeExtensions
	{
		public static ClassIDType ToClassIDType(this Type _this)
		{
			if (_this == typeof(IUnityObjectBase))
				return ClassIDType.Object;

			if (Enum.TryParse(_this.Name, out ClassIDType classID))
			{
				return classID;
			}
			else if (_this.IsInterface && Enum.TryParse(_this.Name.Substring(1), out classID))
			{
				return classID;
			}

			throw new Exception($"{_this} is not Engine's class type");
		}
	}
}
