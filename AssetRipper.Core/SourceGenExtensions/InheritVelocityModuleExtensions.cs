using AssetRipper.SourceGenerated.Subclasses.InheritVelocityModule;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class InheritVelocityModuleExtensions
	{
		public enum InheritVelocityMode
		{
			Initial = 0,
			Current = 1,
		}
		public static InheritVelocityMode GetMode(this IInheritVelocityModule module)
		{
			return (InheritVelocityMode)module.Mode;
		}
	}
}
