using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.Layout.AnimationClips;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct PPtrCurve : IAsset, IDependent
	{
		public PPtrCurve(PPtrCurve copy, IReadOnlyList<PPtrKeyframe> keyframes) :
			this(copy.Path, copy.Attribute, copy.ClassID, copy.Script, keyframes)
		{
		}

		public PPtrCurve(string path, string attribute, ClassIDType classID, PPtr<MonoScript> script)
		{
			Attribute = attribute;
			Path = path;
			ClassID = classID;
			Script = script;
			Curve = null;
		}

		public PPtrCurve(string path, string attribute, ClassIDType classID, PPtr<MonoScript> script, IReadOnlyList<PPtrKeyframe> keyframes) :
			this(path, attribute, classID, script)
		{
			Curve = new PPtrKeyframe[keyframes.Count];
			for (int i = 0; i < keyframes.Count; i++)
			{
				Curve[i] = keyframes[i];
			}
		}

		public static bool operator ==(PPtrCurve left, PPtrCurve right)
		{
			if (left.Attribute != right.Attribute)
			{
				return false;
			}
			if (left.Path != right.Path)
			{
				return false;
			}
			if (left.ClassID != right.ClassID)
			{
				return false;
			}
			if (left.Script != right.Script)
			{
				return false;
			}
			return true;
		}

		public static bool operator !=(PPtrCurve left, PPtrCurve right)
		{
			if (left.Attribute != right.Attribute)
			{
				return true;
			}
			if (left.Path != right.Path)
			{
				return true;
			}
			if (left.ClassID != right.ClassID)
			{
				return true;
			}
			if (left.Script != right.Script)
			{
				return true;
			}
			return false;
		}

		public void Read(AssetReader reader)
		{
			PPtrCurveLayout layout = reader.Layout.AnimationClip.PPtrCurve;
			Curve = reader.ReadAssetArray<PPtrKeyframe>();
			if (layout.IsAlignCurve)
			{
				reader.AlignStream();
			}

			Attribute = reader.ReadString();
			Path = reader.ReadString();
			ClassID = (ClassIDType)reader.ReadInt32();
			Script.Read(reader);
		}

		public void Write(AssetWriter writer)
		{
			PPtrCurveLayout layout = writer.Layout.AnimationClip.PPtrCurve;
			Curve.Write(writer);
			if (layout.IsAlignCurve)
			{
				writer.AlignStream();
			}

			writer.Write(Attribute);
			writer.Write(Path);
			writer.Write((int)ClassID);
			Script.Write(writer);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			PPtrCurveLayout layout = container.ExportLayout.AnimationClip.PPtrCurve;
			node.Add(layout.CurveName, Curve.ExportYAML(container));
			node.Add(layout.AttributeName, Attribute);
			node.Add(layout.PathName, Path);
			node.Add(layout.ClassIDName, (int)ClassID);
			node.Add(layout.ScriptName, Script.ExportYAML(container));
			return node;
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			PPtrCurveLayout layout = context.Layout.AnimationClip.PPtrCurve;
			foreach (PPtr<Object> asset in context.FetchDependencies(Curve, layout.CurveName))
			{
				yield return asset;
			}
			yield return context.FetchDependency(Script, layout.ScriptName);
		}

		public override bool Equals(object obj)
		{
			if (obj is PPtrCurve pptrCurve)
			{
				return this == pptrCurve;
			}
			return false;
		}

		public override int GetHashCode()
		{
			int hash = 113;
			unchecked
			{
				hash = hash + 457 * Attribute.GetHashCode();
				hash = hash * 433 + Path.GetHashCode();
				hash = hash * 223 + ClassID.GetHashCode();
				hash = hash * 911 + Script.GetHashCode();
			}
			return hash;
		}

		public PPtrKeyframe[] Curve { get; set; }
		public string Attribute { get; set; }
		public string Path { get; set; }
		public ClassIDType ClassID { get; set; }

		public PPtr<MonoScript> Script;
	}
}
