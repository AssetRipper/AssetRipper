using System;

namespace uTinyRipper.Classes
{
	public static class UtinyClassTypeExtensions
	{
		public static Type ToType(this ClassIDType _this)
		{
			switch (_this)
			{
				case ClassIDType.Object:
					return typeof(Object);

				case ClassIDType.GameObject:
					return typeof(GameObject);

				case ClassIDType.Component:
					return typeof(Component);

				case ClassIDType.Transform:
					return typeof(Transform);

				case ClassIDType.Material:
					return typeof(Material);

				case ClassIDType.Shader:
					return typeof(Shader);

				default:
					throw new Exception($"Unsupported class type {_this}");
			}
		}
	}
}