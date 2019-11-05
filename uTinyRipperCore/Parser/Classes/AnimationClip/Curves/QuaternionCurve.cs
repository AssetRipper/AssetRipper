using System.Collections.Generic;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct QuaternionCurve : IAssetReadable, IYAMLExportable
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

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			context.AddNode(nameof(QuaternionCurve), name);
			context.BeginChildren();
			AnimationCurveTpl<Quaternionf>.GenerateTypeTree(context, CurveName, Quaternionf.GenerateTypeTree);
			context.AddString(PathName);
			context.EndChildren();
		}

		public void Read(AssetReader reader)
		{
			Curve.Read(reader);
			Path = reader.ReadString();
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

		public const string CurveName = "curve";
		public const string PathName = "path";

		public AnimationCurveTpl<Quaternionf> Curve;
	}
}
