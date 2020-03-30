using System;

namespace uTinyRipper.Classes
{
	public static class ObjectExtensions
	{
		public static string GetOriginalName(this Object _this)
		{
			if (_this is NamedObject named)
			{
				return named.Name;
			}
			else if (_this is GameObject gameObject)
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

		public static string TryGetName(this Object _this)
		{
			if (_this is NamedObject named)
			{
				return named.ValidName;
			}
			else if (_this is GameObject gameObject)
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
