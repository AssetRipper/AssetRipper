using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_198;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.SubEmitterData;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class SubEmitterDataExtensions
	{
		public static void SetValues(this ISubEmitterData data, ParticleSystemSubEmitterType type, PPtr<IParticleSystem> emitter)
		{
			data.Emitter.CopyValues(emitter);
			data.Type = (int)type;
			data.Properties = (int)ParticleSystemSubEmitterProperties.InheritNothing;
			data.EmitProbability = 1.0f;
		}

		public static ParticleSystemSubEmitterType GetSubEmitterType(this ISubEmitterData data)
		{
			return (ParticleSystemSubEmitterType)data.Type;
		}

		public static ParticleSystemSubEmitterProperties GetProperties(this ISubEmitterData data)
		{
			return (ParticleSystemSubEmitterProperties)data.Properties;
		}
	}
}
