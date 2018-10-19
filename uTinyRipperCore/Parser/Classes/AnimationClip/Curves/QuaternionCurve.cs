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

		public QuaternionCurve(string path, AnimationCurveTpl<Quaternionf> curve)
		{
			Path = path;
			Curve = curve;
		}

		public void Read(AssetReader reader)
		{
			Curve.Read(reader);
			Path = reader.ReadString();
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
