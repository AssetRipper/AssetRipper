using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.CustomDataModule;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class CustomDataModuleExtensions
	{
		public static ParticleSystemCustomDataMode GetMode0(this ICustomDataModule module)
		{
			return (ParticleSystemCustomDataMode)module.Mode0;
		}

		public static ParticleSystemCustomDataMode GetMode1(this ICustomDataModule module)
		{
			return (ParticleSystemCustomDataMode)module.Mode1;
		}
	}
}
