using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;
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
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

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
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Emitter.FetchDependency(file, isLog, () => nameof(SubEmitterData), "emitter");
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("emitter", Emitter.ExportYAML(container));
			node.Add("type", (int)Type);
			node.Add("properties", (int)Properties);
			return node;
		}

		public ParticleSystemSubEmitterType Type { get; private set; }
		public ParticleSystemSubEmitterProperties Properties { get; private set; }

		public PPtr<ParticleSystem> Emitter;
	}
}
