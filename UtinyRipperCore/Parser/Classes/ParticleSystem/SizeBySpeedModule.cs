using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct SizeBySpeedModule : IAssetReadable, IYAMLExportable
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
			
			Curve.Read(stream);
			Y.Read(stream);
			Z.Read(stream);
			Range.Read(stream);
			SeparateAxes = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
			
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("enabled", Enabled);
			node.Add("curve", Curve.ExportYAML(exporter));
			node.Add("y", Y.ExportYAML(exporter));
			node.Add("z", Z.ExportYAML(exporter));
			node.Add("range", Range.ExportYAML(exporter));
			node.Add("separateAxes", SeparateAxes);
			return node;
		}

		public bool Enabled { get; private set; }
		public bool SeparateAxes { get; private set; }

		public MinMaxCurve Curve;
		public MinMaxCurve Y;
		public MinMaxCurve Z;
		public Vector2f Range;
	}
}
