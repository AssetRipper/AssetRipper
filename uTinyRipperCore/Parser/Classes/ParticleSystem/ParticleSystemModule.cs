using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public abstract class ParticleSystemModule : IAssetReadable, IYAMLExportable
	{
		protected ParticleSystemModule()
		{
		}

		protected ParticleSystemModule(bool enabled)
		{
			Enabled = enabled;
		}

		public virtual void Read(AssetReader reader)
		{
			Enabled = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
		}

		public virtual YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("enabled", Enabled);
			return node;
		}

		public bool Enabled { get; protected set; }
	}
}
