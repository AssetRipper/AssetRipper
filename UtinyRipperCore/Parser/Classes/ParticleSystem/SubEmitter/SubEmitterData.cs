using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct SubEmitterData : IAssetReadable, IYAMLExportable, IDependent
	{
		public SubEmitterData(PPtr<ParticleSystem> emitter, ParticleSystemSubEmitterType type)
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
#warning TODO: values acording to read version (current 2017.3.0f3)
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
