using AssetRipper.Core.Classes.Misc.KeyframeTpl;
using AssetRipper.Core.Classes.Misc.Serializable.AnimationCurveTpl;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.AnimationClip.Curves
{
	public sealed class QuaternionCurve : IAsset, IYAMLExportable
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

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(CurveName, Curve.ExportYAML(container));
			node.Add(PathName, Path);
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

		public AnimationCurveTpl<Quaternionf> Curve = new();
		public const string CurveName = "curve";
		public const string PathName = "path";
	}
}
