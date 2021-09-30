using AssetRipper.Core.Classes;
using System;

namespace AssetRipper.Core.Extensions
{
	public static class ObjectExtensions
	{
		public static string GetOriginalName(this Classes.Object.Object _this)
		{
			if (_this is NamedObject named)
			{
				return named.Name;
			}
			else if (_this is Classes.GameObject.GameObject gameObject)
			{
				return gameObject.Name;
			}
			else if (_this is MonoBehaviour monoBeh)
			{
				return monoBeh.Name;
			}
			else
			{
				throw new Exception($"Unable to get name for {_this.ClassID}");
			}
		}

		public static string TryGetName(this Classes.Object.Object _this)
		{
			if (_this is NamedObject named)
			{
				return named.ValidName;
			}
			else if (_this is Classes.GameObject.GameObject gameObject)
			{
				return gameObject.Name;
			}
			else if (_this is MonoBehaviour monoBeh)
			{
				return monoBeh.Name;
			}
			else
			{
				return null;
			}
		}
	}
}
