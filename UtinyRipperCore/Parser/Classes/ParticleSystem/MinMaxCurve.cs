using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.AnimationClips;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct MinMaxCurve : IAssetReadable, IYAMLExportable
	{
		private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}

		public void Read(AssetStream stream)
		{
			MinMaxState = stream.ReadUInt16();
			stream.AlignStream(AlignType.Align4);
			
			Scalar = stream.ReadSingle();
			MinScalar = stream.ReadSingle();
			MaxCurve.Read(stream);
			MinCurve.Read(stream);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("minMaxState", MinMaxState);
			node.Add("scalar", Scalar);
			node.Add("minScalar", MinScalar);
			node.Add("maxCurve", MaxCurve.ExportYAML(exporter));
			node.Add("minCurve", MinCurve.ExportYAML(exporter));
			return node;
		}

		public ushort MinMaxState { get; private set; }
		public float Scalar { get; private set; }
		public float MinScalar { get; private set; }

		public AnimationCurveTpl<Float> MaxCurve;
		public AnimationCurveTpl<Float> MinCurve;
	}
}
