using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimationClips
{
	public struct QuaternionCurve : IAssetReadable, IYAMLExportable
	{
		public QuaternionCurve(string path)
		{
			Curve = new AnimationCurveTpl<Quaternionf>(2, 2, 4);
			Path = path;
		}

		public void Read(AssetStream stream)
		{
			Curve.Read(stream);
			Path = stream.ReadStringAligned();
		}
		
		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("curve", Curve.ExportYAML(exporter));
			node.Add("path", Path);

			return node;
		}

		public string Path { get; set; }

		public AnimationCurveTpl<Quaternionf> Curve;
	}
}
