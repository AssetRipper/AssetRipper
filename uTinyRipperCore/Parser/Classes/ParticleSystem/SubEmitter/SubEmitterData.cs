using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

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

		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadEmitProbability(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}

		private static int GetSerializedVersion(Version version)
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

		public void Read(AssetReader reader)
		{
			Emitter.Read(reader);
			Type = (ParticleSystemSubEmitterType)reader.ReadInt32();
			Properties = (ParticleSystemSubEmitterProperties)reader.ReadInt32();
			if (IsReadEmitProbability(reader.Version))
			{
				EmitProbability = reader.ReadSingle();
			}
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Emitter.FetchDependency(file, isLog, () => nameof(SubEmitterData), "emitter");
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(EmitterName, Emitter.ExportYAML(container));
			node.Add(TypeName, (int)Type);
			node.Add(PropertiesName, (int)Properties);
			if (IsReadEmitProbability(container.ExportVersion))
			{
				node.Add(EmitProbabilityName, EmitProbability);
			}
			return node;
		}

		public ParticleSystemSubEmitterType Type { get; private set; }
		public ParticleSystemSubEmitterProperties Properties { get; private set; }
		public float EmitProbability { get; private set; }

		public const string EmitterName = "emitter";
		public const string TypeName = "type";
		public const string PropertiesName = "properties";
		public const string EmitProbabilityName = "emitProbability";

		public PPtr<ParticleSystem> Emitter;
	}
}
