using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimationClips
{
	public struct Vector3Curve : IAssetReadable, IYAMLExportable
	{
		public Vector3Curve(string path)
		{
			Curve = new AnimationCurveTpl<Vector3f>(true);
			Path = path;
		}

		public void Read(AssetStream stream)
		{
			Curve.Read(stream);
			Path = stream.ReadStringAligned();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("curve", Curve.ExportYAML(container));
			node.Add("path", Path);
			return node;
		}

		public string Path { get; private set; }

		public AnimationCurveTpl<Vector3f> Curve;
	}
}
