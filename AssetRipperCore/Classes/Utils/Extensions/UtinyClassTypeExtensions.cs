using AssetRipper.Core.Parser.Asset;
using System;

namespace AssetRipper.Core.Classes.Utils.Extensions
{
	public static class AssetClassTypeExtensions
	{
		public static Type ToType(this ClassIDType _this)
		{
			switch (_this)
			{
				case ClassIDType.Object:
					return typeof(Object.Object);

				case ClassIDType.GameObject:
					return typeof(GameObject.GameObject);

				case ClassIDType.Component:
					return typeof(Component);

				case ClassIDType.Transform:
					return typeof(Transform);

				case ClassIDType.Material:
					return typeof(Material.Material);

				case ClassIDType.Shader:
					return typeof(Shader.Shader);

				default:
					throw new Exception($"Unsupported class type {_this}");
			}
		}
	}
}