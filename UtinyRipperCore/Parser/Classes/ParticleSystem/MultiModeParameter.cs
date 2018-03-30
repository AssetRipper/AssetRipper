using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct MultiModeParameter : IAssetReadable, IYAMLExportable
	{
		/*private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		public void Read(AssetStream stream)
		{
			Value = stream.ReadSingle();
			Mode = stream.ReadInt32();
			Spread = stream.ReadSingle();
			Speed.Read(stream);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("value", Value);
			node.Add("mode", Mode);
			node.Add("spread", Spread);
			node.Add("speed", Speed.ExportYAML(exporter));
			return node;
		}

		public float Value { get; private set; }
		public int Mode { get; private set; }
		public float Spread { get; private set; }

		public MinMaxCurve Speed;
	}
}
