using AssetRipper.Core.Classes.ParticleSystem.Shape;
using AssetRipper.SourceGenerated.Subclasses.ShapeModule;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class ShapeModuleExtensions
	{
		public static ParticleSystemShapeType GetShapeType(this IShapeModule module)
		{
			return (ParticleSystemShapeType)module.Type;
		}

		public static PlacementMode GetPlacementMode(this IShapeModule module)
		{
			return (PlacementMode)module.PlacementMode;
		}

		private static ParticleSystemShapeType GetExportType(this IShapeModule module)
		{
			if (module.Has_RadiusThickness())
			{
				return module.GetShapeType();
			}
			return module.GetShapeType() switch
			{
				ParticleSystemShapeType.SphereShell => ParticleSystemShapeType.Sphere,
				ParticleSystemShapeType.HemisphereShell => ParticleSystemShapeType.Hemisphere,
				ParticleSystemShapeType.ConeShell => ParticleSystemShapeType.Cone,
				ParticleSystemShapeType.ConeVolumeShell => ParticleSystemShapeType.ConeVolume,
				ParticleSystemShapeType.CircleEdge => ParticleSystemShapeType.Circle,
				_ => module.GetShapeType(),
			};
		}

		private static float GetExportRadiusThickness(this IShapeModule module)
		{
			if (module.Has_RadiusThickness())
			{
				return module.RadiusThickness;
			}

			switch (module.GetShapeType())
			{
				case ParticleSystemShapeType.SphereShell:
				case ParticleSystemShapeType.HemisphereShell:
				case ParticleSystemShapeType.ConeShell:
				case ParticleSystemShapeType.ConeVolumeShell:
				case ParticleSystemShapeType.CircleEdge:
					return 0.0f;

				default:
					return 1.0f;
			}
		}
	}
}
