using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.ParticleSystem
{
	public abstract class ParticleSystemModule : IAssetReadable, IYamlExportable
	{
		protected ParticleSystemModule() { }

		protected ParticleSystemModule(bool enabled)
		{
			Enabled = enabled;
		}

		public virtual void Read(AssetReader reader)
		{
			Enabled = reader.ReadBoolean();
			reader.AlignStream();
		}

		public virtual YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(EnabledName, Enabled);
			return node;
		}

		public bool Enabled { get; protected set; }

		public const string EnabledName = "enabled";
	}
}
