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
					case ClassIDType.Texture:
					case ClassIDType.Texture2D:
					case ClassIDType.Material:
					case ClassIDType.Mesh:
					case ClassIDType.Shader:
					case ClassIDType.Rigidbody2D:
					case ClassIDType.Rigidbody:
					case ClassIDType.Collider:
					case ClassIDType.PhysicsMaterial2D:
					case ClassIDType.AnimationClip:
					case ClassIDType.AudioClip:
					case ClassIDType.RenderTexture:
					case ClassIDType.Cubemap:
					case ClassIDType.Avatar:
					case ClassIDType.RuntimeAnimatorController:
					case ClassIDType.BaseAnimationTrack:
					case ClassIDType.MonoBehaviour:
					case ClassIDType.MonoScript:
					case ClassIDType.Flare:
					case ClassIDType.Font:
					case ClassIDType.PhysicMaterial:
					case ClassIDType.TerrainData:
					case ClassIDType.Sprite:
					case ClassIDType.AudioMixerGroup:
					case ClassIDType.OcclusionCullingData:
					case ClassIDType.SceneAsset:
					case ClassIDType.LightmapParameters:
						return classID;

					default:
						throw new Exception($"Unsupported  type {_this}");
				}
			}

			throw new Exception($"{_this} is not Engine class type");
		}
	}
}
