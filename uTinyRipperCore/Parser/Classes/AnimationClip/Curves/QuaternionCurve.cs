using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct QuaternionCurve : IAssetReadable, IYAMLExportable
	{
		public QuaternionCurve(string path, IReadOnlyList<KeyframeTpl<Quaternionf>> keyframes)
		{
			Curve = new AnimationCurveTpl<Quaternionf>(keyframes);
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
