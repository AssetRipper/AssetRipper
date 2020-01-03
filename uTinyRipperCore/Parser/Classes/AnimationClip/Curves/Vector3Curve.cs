using System.Collections.Generic;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.Layout.AnimationClips;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct Vector3Curve : IAsset, IYAMLExportable
	{
		public Vector3Curve(Vector3Curve copy, IReadOnlyList<KeyframeTpl<Vector3f>> keyframes):
			this(copy.Path, keyframes)
		{
		}

		public Vector3Curve(string path)
		{
			Path = path;
			Curve = new AnimationCurveTpl<Vector3f>(false);
		}

		public Vector3Curve(string path, IReadOnlyList<KeyframeTpl<Vector3f>> keyframes)
		{
			Path = path;
			Curve = new AnimationCurveTpl<Vector3f>(keyframes);
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
			Vector3CurveLayout layout = container.ExportLayout.AnimationClip.Vector3Curve;
			node.Add(layout.CurveName, Curve.ExportYAML(container));
			node.Add(layout.PathName, Path);
			return node;
		}

		public override int GetHashCode()
		{
			int hash = 577;
			unchecked
			{
				hash = 419 + hash * Path.GetHashCode();
			}
			return hash;
		}

		public string Path { get; set; }

		public AnimationCurveTpl<Vector3f> Curve;
	}
}
