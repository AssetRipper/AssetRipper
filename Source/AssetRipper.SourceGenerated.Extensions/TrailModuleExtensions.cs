using AssetRipper.SourceGenerated.Subclasses.TrailModule;

namespace AssetRipper.SourceGenerated.Extensions;

public static class TrailModuleExtensions
{
	public static void SetToDefault(this ITrailModule module, UnityVersion version)
	{
		module.Ratio = 1.0f;
		module.Lifetime.SetValues(version, 1.0f);
		module.MinVertexDistance = 0.2f;
		module.RibbonCount = 1;
		module.DieWithParticles = true;
		module.SizeAffectsWidth = true;
		module.InheritParticleColor = true;
		module.ColorOverLifetime.SetToDefault();
		module.WidthOverTrail.SetValues(version, 1.0f);
		module.ColorOverTrail.SetToDefault();
	}
}
