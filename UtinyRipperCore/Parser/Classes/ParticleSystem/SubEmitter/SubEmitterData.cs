using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct SubEmitterData : IAssetReadable, IYAMLExportable
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

		public void Read(AssetStream stream)
		{
			Emitter.Read(stream);
			Type = (ParticleSystemSubEmitterType)stream.ReadInt32();
			Properties = (ParticleSystemSubEmitterProperties)stream.ReadInt32();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("emitter", Emitter.ExportYAML(exporter));
			node.Add("type", (int)Type);
			node.Add("properties", (int)Properties);
			return node;
		}

		public ParticleSystemSubEmitterType Type { get; private set; }
		public ParticleSystemSubEmitterProperties Properties { get; private set; }

		public PPtr<ParticleSystem> Emitter;
	}
}
