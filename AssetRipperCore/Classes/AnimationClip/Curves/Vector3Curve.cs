using AssetRipper.Core.Classes.Misc.KeyframeTpl;
using AssetRipper.Core.Classes.Misc.Serializable.AnimationCurveTpl;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.AnimationClip.Curves
{
	public sealed class Vector3Curve : IAsset, IYamlExportable, IEquatable<Vector3Curve>
	{
		public Vector3Curve() { }

		public Vector3Curve(Vector3Curve copy, IReadOnlyList<KeyframeTpl<Vector3f>> keyframes) : this(copy.Path, keyframes) { }

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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(CurveName, Curve.ExportYaml(container));
			node.Add(PathName, Path);
			return node;
		}

		public override bool Equals(object obj)
		{
			if (obj is Vector3Curve curve)
				return Equals(curve);
			return false;
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public bool Equals(Vector3Curve other)
		{
			return Path.Equals(other.Path) && Curve.Equals(other.Curve);
		}

		public string Path { get; set; }

		public AnimationCurveTpl<Vector3f> Curve = new();
		public const string CurveName = "curve";
		public const string PathName = "path";
	}
}
