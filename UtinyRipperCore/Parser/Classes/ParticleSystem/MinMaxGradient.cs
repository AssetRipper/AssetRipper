using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct MinMaxGradient : IAssetReadable, IYAMLExportable
	{
		/*private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		public void Read(AssetStream stream)
		{
			MinMaxState = stream.ReadUInt16();
			stream.AlignStream(AlignType.Align4);
			
			MinColor.Read(stream);
			MaxColor.Read(stream);
			MaxGradient.Read(stream);
			MinGradient.Read(stream);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("minMaxState", MinMaxState);
			node.Add("minColor", MinColor.ExportYAML(exporter));
			node.Add("maxColor", MaxColor.ExportYAML(exporter));
			node.Add("maxGradient", MaxGradient.ExportYAML(exporter));
			node.Add("minGradient", MinGradient.ExportYAML(exporter));
			return node;
		}

		public ushort MinMaxState { get; private set; }

		public ColorRGBAf MinColor;
		public ColorRGBAf MaxColor;
		public Gradient MaxGradient;
		public Gradient MinGradient;
	}
}
