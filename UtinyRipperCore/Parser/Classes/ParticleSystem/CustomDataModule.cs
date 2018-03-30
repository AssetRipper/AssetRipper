using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct CustomDataModule : IAssetReadable, IYAMLExportable
	{
		/*private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		public void Read(AssetStream stream)
		{
			Enabled = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
			
			Mode0 = stream.ReadInt32();
			VectorComponentCount0 = stream.ReadInt32();
			Color0.Read(stream);
			Vector0_0.Read(stream);
			Vector0_1.Read(stream);
			Vector0_2.Read(stream);
			Vector0_3.Read(stream);
			Mode1 = stream.ReadInt32();
			VectorComponentCount1 = stream.ReadInt32();
			Color1.Read(stream);
			Vector1_0.Read(stream);
			Vector1_1.Read(stream);
			Vector1_2.Read(stream);
			Vector1_3.Read(stream);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("enabled", Enabled);
			node.Add("mode0", Mode0);
			node.Add("vectorComponentCount0", VectorComponentCount0);
			node.Add("color0", Color0.ExportYAML(exporter));
			node.Add("vector0_0", Vector0_0.ExportYAML(exporter));
			node.Add("vector0_1", Vector0_1.ExportYAML(exporter));
			node.Add("vector0_2", Vector0_2.ExportYAML(exporter));
			node.Add("vector0_3", Vector0_3.ExportYAML(exporter));
			node.Add("mode1", Mode1);
			node.Add("vectorComponentCount1", VectorComponentCount1);
			node.Add("color1", Color1.ExportYAML(exporter));
			node.Add("vector1_0", Vector1_0.ExportYAML(exporter));
			node.Add("vector1_1", Vector1_1.ExportYAML(exporter));
			node.Add("vector1_2", Vector1_2.ExportYAML(exporter));
			node.Add("vector1_3", Vector1_3.ExportYAML(exporter));
			return node;
		}

		public bool Enabled { get; private set; }
		public int Mode0 { get; private set; }
		public int VectorComponentCount0 { get; private set; }
		public int Mode1 { get; private set; }
		public int VectorComponentCount1 { get; private set; }

		public MinMaxGradient Color0;
		public MinMaxCurve Vector0_0;
		public MinMaxCurve Vector0_1;
		public MinMaxCurve Vector0_2;
		public MinMaxCurve Vector0_3;
		public MinMaxGradient Color1;
		public MinMaxCurve Vector1_0;
		public MinMaxCurve Vector1_1;
		public MinMaxCurve Vector1_2;
		public MinMaxCurve Vector1_3;
	}
}
