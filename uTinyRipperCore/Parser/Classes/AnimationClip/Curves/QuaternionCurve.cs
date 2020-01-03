using System.Collections.Generic;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.Layout.AnimationClips;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct QuaternionCurve : IAsset, IYAMLExportable
	{
		public QuaternionCurve(QuaternionCurve copy, IReadOnlyList<KeyframeTpl<Quaternionf>> keyframes) :
			this(copy.Path, keyframes)
		{
		}

		public QuaternionCurve(string path)
		{
			Path = path;
			Curve = new AnimationCurveTpl<Quaternionf>(false);
		}

		public QuaternionCurve(string path, IReadOnlyList<KeyframeTpl<Quaternionf>> keyframes)
		{
			Path = path;
			Curve = new AnimationCurveTpl<Quaternionf>(keyframes);
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

		public void Write(AssetWriter writer)
		{
			Curve.Write(writer);
			writer.Write(Path);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			QuaternionCurveLayout layout = container.ExportLayout.AnimationClip.QuaternionCurve;
			node.Add(layout.CurveName, Curve.ExportYAML(container));
			node.Add(layout.PathName, Path);
			return node;
		}

		public override int GetHashCode()
		{
			int hash = 199;
			unchecked
			{
				hash = 617 + hash * Path.GetHashCode();
			}
			return hash;
		}

		public string Path { get; set; }

		public AnimationCurveTpl<Quaternionf> Curve;
	}
}
