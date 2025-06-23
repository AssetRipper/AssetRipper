using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.ShapeModule;

namespace AssetRipper.SourceGenerated.Extensions;

public static class ShapeModuleExtensions
{
	public enum PlacementMode
	{
		BoxVolume = 0,
		ConeBase = 0,
		MeshVertex = 0,
		BoxShell = 1,
		ConeVolume = 1,
		MeshEdge = 1,
		BoxEdge = 2,
		MeshTriangle = 2,
	}

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
