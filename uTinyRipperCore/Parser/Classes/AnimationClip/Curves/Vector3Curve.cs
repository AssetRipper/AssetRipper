using System.Collections.Generic;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct Vector3Curve : IAssetReadable, IYAMLExportable
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

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			context.AddNode(nameof(Vector3Curve), name);
			context.BeginChildren();
			AnimationCurveTpl<Vector3f>.GenerateTypeTree(context, name, Vector3f.GenerateTypeTree);
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
			int hash = 577;
			unchecked
			{
				hash = 419 + hash * Path.GetHashCode();
			}
			return hash;
		}

		public string Path { get; set; }

		public const string CurveName = "curve";
		public const string PathName = "path";

		public AnimationCurveTpl<Vector3f> Curve;
	}
}
