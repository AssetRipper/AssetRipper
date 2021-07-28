using AssetRipper.Parser.Asset;
using System;

namespace AssetRipper.Classes.Utils.Extensions
{
	public static class TypeExtensions
	{
		public static ClassIDType ToClassIDType(this Type _this)
		{
			if (Enum.TryParse(_this.Name, out ClassIDType classID))
			{
				return classID;
			}

			throw new Exception($"{_this} is not Engine's class type");
		}
	}
}
