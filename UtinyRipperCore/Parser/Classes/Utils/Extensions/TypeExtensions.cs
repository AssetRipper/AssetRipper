using System;

namespace UtinyRipper.Classes
{
	public static class TypeExtensions
	{
		public static ClassIDType ToClassIDType(this Type _this)
		{
			if (Enum.TryParse(_this.Name, out ClassIDType classID))
			{
				switch(classID)
				{
					case ClassIDType.Object:
					case ClassIDType.GameObject:
					case ClassIDType.Component:
					case ClassIDType.Transform:
					case ClassIDType.Camera:
					case ClassIDType.Material:
					case ClassIDType.Shader:
					case ClassIDType.Mesh:
					case ClassIDType.Texture:
					case ClassIDType.AnimationClip:
					case ClassIDType.AudioClip:
					case ClassIDType.Avatar:
					case ClassIDType.RuntimeAnimatorController:
					case ClassIDType.BaseAnimationTrack:
					case ClassIDType.MonoBehaviour:
					case ClassIDType.MonoScript:
					case ClassIDType.Flare:
					case ClassIDType.Font:
					case ClassIDType.Sprite:
						return classID;

					default:
						throw new Exception($"Unsupported  type {_this}");
				}
			}

			throw new Exception($"{_this} is not Engine class type");
		}
	}
}
