using AssetRipper.Core.Classes.Misc.KeyframeTpl;
using AssetRipper.Core.Classes.Misc.Serializable.AnimationCurveTpl;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.AnimationClip.Curves
{
	public sealed class QuaternionCurve : IAsset, IYamlExportable, IEquatable<QuaternionCurve>
	{
		public QuaternionCurve() { }

		public QuaternionCurve(QuaternionCurve copy, IReadOnlyList<KeyframeTpl<Quaternionf>> keyframes) : this(copy.Path, keyframes) { }

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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(CurveName, Curve.ExportYaml(container));
			node.Add(PathName, Path);
			return node;
		}

		public override bool Equals(object obj)
		{
			if(obj is QuaternionCurve curve)
				return Equals(curve);
			return false;
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public bool Equals(QuaternionCurve other)
		{
			return Path.Equals(other.Path) && Curve.Equals(other.Curve);
		}

		public string Path { get; set; }

		public AnimationCurveTpl<Quaternionf> Curve = new();
		public const string CurveName = "curve";
		public const string PathName = "path";
	}
}
