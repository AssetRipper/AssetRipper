using AssetRipper.Core.Classes.ParticleSystem.InheritVelocity;
using AssetRipper.SourceGenerated.Subclasses.InheritVelocityModule;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class InheritVelocityModuleExtensions
	{
		public static InheritVelocityMode GetMode(this IInheritVelocityModule module)
		{
			return (InheritVelocityMode)module.Mode;
		}
	}
}
