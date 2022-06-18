using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.ParticleSystem.SubEmitter;
using AssetRipper.SourceGenerated.Classes.ClassID_198;
using AssetRipper.SourceGenerated.Subclasses.SubEmitterData;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class SubEmitterDataExtensions
	{
		public static void SetValues(this ISubEmitterData data, ParticleSystemSubEmitterType type, IPPtr<IParticleSystem> emitter)
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
