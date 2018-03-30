using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct SubEmitterData : IAssetReadable, IYAMLExportable
	{
		/*private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		public void Read(AssetStream stream)
		{
			Emitter.Read(stream);
			Type = stream.ReadInt32();
			Properties = stream.ReadInt32();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("emitter", Emitter.ExportYAML(exporter));
			node.Add("type", Type);
			node.Add("properties", Properties);
			return node;
		}

		public int Type { get; private set; }
		public int Properties { get; private set; }

		public PPtr<ParticleSystem> Emitter;
	}
}
