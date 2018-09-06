using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimationClips
{
	public struct QuaternionCurve : IAssetReadable, IYAMLExportable
	{
		public QuaternionCurve(string path)
		{
			Curve = new AnimationCurveTpl<Quaternionf>(true);
			Path = path;
		}

		public void Read(AssetReader reader)
		{
			Curve.Read(reader);
			Path = reader.ReadStringAligned();
		}
		
		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("curve", Curve.ExportYAML(container));
			node.Add("path", Path);

			return node;
		}

		public string Path { get; set; }

		public AnimationCurveTpl<Quaternionf> Curve;
	}
}
