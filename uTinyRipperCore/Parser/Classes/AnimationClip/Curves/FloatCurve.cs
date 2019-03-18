using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

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
		public static bool IsReadScript(Version version)
		{
			return version.IsGreaterEqual(2);
		}

		public void Read(AssetReader reader)
		{
			Curve.Read(reader);
			Attribute = reader.ReadString();
			Path = reader.ReadString();
			ClassID = (ClassIDType)reader.ReadInt32();
			if (IsReadScript(reader.Version))
			{
				Script.Read(reader);
			}
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Script.FetchDependency(file, isLog, () => nameof(FloatCurve), "script");
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("curve", Curve.ExportYAML(container));
			node.Add("attribute", Attribute);
			node.Add("path", Path);
			node.Add("classID", (int)ClassID);
			node.Add("script", Script.ExportYAML(container));
			return node;
		}
		
		public string Attribute { get; private set; }
		public string Path { get; private set; }
		public ClassIDType ClassID { get; private set; }

		public AnimationCurveTpl<Float> Curve;
		public PPtr<MonoScript> Script;
	}
}
