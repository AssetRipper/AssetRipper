using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes.ParticleSystems
{
	public struct SubEmitterData : IAssetReadable, IYAMLExportable, IDependent
	{
		public SubEmitterData(ParticleSystemSubEmitterType type, PPtr<ParticleSystem> emitter)
		{
			Emitter = emitter;
			Type = type;
			Properties = ParticleSystemSubEmitterProperties.InheritNothing;
			EmitProbability = 1.0f;
		}

		public static int ToSerializedVersion(Version version)
		{
			// ParticleSystemSubEmitterProperties.InheritDuration added
			if (version.IsGreaterEqual(2018, 3))
			{
				return 3;
			}
			// ParticleSystemSubEmitterProperties.InheritLifetime added
			if (version.IsGreaterEqual(2017, 2))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasEmitProbability(Version version) => version.IsGreaterEqual(2018, 3);

		public void Read(AssetReader reader)
		{
			Emitter.Read(reader);
			Type = (ParticleSystemSubEmitterType)reader.ReadInt32();
			Properties = (ParticleSystemSubEmitterProperties)reader.ReadInt32();
			if (HasEmitProbability(reader.Version))
			{
				EmitProbability = reader.ReadSingle();
			}
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Emitter, EmitterName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(EmitterName, Emitter.ExportYAML(container));
			node.Add(TypeName, (int)Type);
			node.Add(PropertiesName, (int)Properties);
			if (HasEmitProbability(container.ExportVersion))
			{
				node.Add(EmitProbabilityName, EmitProbability);
			}
			return node;
		}

		public ParticleSystemSubEmitterType Type { get; set; }
		public ParticleSystemSubEmitterProperties Properties { get; set; }
		public float EmitProbability { get; set; }

		public const string EmitterName = "emitter";
		public const string TypeName = "type";
		public const string PropertiesName = "properties";
		public const string EmitProbabilityName = "emitProbability";

		public PPtr<ParticleSystem> Emitter;
	}
}
