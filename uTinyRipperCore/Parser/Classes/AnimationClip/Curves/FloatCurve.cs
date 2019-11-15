using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Layout.AnimationClips;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct FloatCurve : IAsset, IDependent
	{
		public FloatCurve(FloatCurve copy, IReadOnlyList<KeyframeTpl<Float>> keyframes):
			this(copy.Path, copy.Attribute, copy.ClassID, copy.Script, keyframes)
		{
		}

		public FloatCurve(string path, string attribute, ClassIDType classID, PPtr<MonoScript> script)
		{
			Path = path;
			Attribute = attribute;
			ClassID = classID;
			Script = script;
			Curve = new AnimationCurveTpl<Float>(false);
		}

		public FloatCurve(string path, string attribute, ClassIDType classID, PPtr<MonoScript> script, IReadOnlyList<KeyframeTpl<Float>> keyframes)
		{
			Path = path;
			Attribute = attribute;
			ClassID = classID;
			Script = script;
			Curve = new AnimationCurveTpl<Float>(keyframes);
		}

		public void Read(AssetReader reader)
		{
			FloatCurveLayout layout = reader.Layout.AnimationClip.FloatCurve;
			Curve.Read(reader);
			Attribute = reader.ReadString();
			Path = reader.ReadString();
			ClassID = (ClassIDType)reader.ReadInt32();
			if (layout.HasScript)
			{
				Script.Read(reader);
			}
		}

		public void Write(AssetWriter writer)
		{
			FloatCurveLayout layout = writer.Layout.AnimationClip.FloatCurve;
			Curve.Write(writer);
			writer.Write(Attribute);
			writer.Write(Path);
			writer.Write((int)ClassID);
			if (layout.HasScript)
			{
				Script.Write(writer);
			}
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			FloatCurveLayout layout = context.Layout.AnimationClip.FloatCurve;
			yield return context.FetchDependency(Script, layout.ScriptName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			FloatCurveLayout layout = container.ExportLayout.AnimationClip.FloatCurve;
			node.Add(layout.CurveName, Curve.ExportYAML(container));
			node.Add(layout.AttributeName, Attribute);
			node.Add(layout.PathName, Path);
			node.Add(layout.ClassIDName, (int)ClassID);
			node.Add(layout.ScriptName, Script.ExportYAML(container));
			return node;
		}
		
		public string Attribute { get; set; }
		public string Path { get; set; }
		public ClassIDType ClassID { get; set; }

		public AnimationCurveTpl<Float> Curve;
		public PPtr<MonoScript> Script;
	}
}
