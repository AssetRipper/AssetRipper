using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct Vector3Curve : IAssetReadable, IYAMLExportable
	{
		public Vector3Curve(string path, IReadOnlyList<KeyframeTpl<Vector3f>> keyframes)
		{
			Curve = new AnimationCurveTpl<Vector3f>(keyframes);
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

		public string Path { get; private set; }

		public AnimationCurveTpl<Vector3f> Curve;
	}
}
