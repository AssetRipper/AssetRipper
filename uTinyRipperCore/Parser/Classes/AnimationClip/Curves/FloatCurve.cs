using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Classes.Misc;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct FloatCurve : IAssetReadable, IYAMLExportable, IDependent
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

		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool HasScript(Version version) => version.IsGreaterEqual(2);

		public void Read(AssetReader reader)
		{
			Curve.Read(reader);
			Attribute = reader.ReadString();
			Path = reader.ReadString();
			ClassID = (ClassIDType)reader.ReadInt32();
			if (HasScript(reader.Version))
			{
				Script.Read(reader);
			}
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Script, ScriptName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(CurveName, Curve.ExportYAML(container));
			node.Add(AttributeName, Attribute);
			node.Add(PathName, Path);
			node.Add(ClassIDName, (int)ClassID);
			node.Add(ScriptName, Script.ExportYAML(container));
			return node;
		}
		
		public string Attribute { get; set; }
		public string Path { get; set; }
		public ClassIDType ClassID { get; set; }

		public const string CurveName = "curve";
		public const string AttributeName = "attribute";
		public const string PathName = "path";
		public const string ClassIDName = "classID";
		public const string ScriptName = "script";

		public AnimationCurveTpl<Float> Curve;
		public PPtr<MonoScript> Script;
	}
}
