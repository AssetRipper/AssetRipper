using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct PPtrCurve : IAssetReadable, IYAMLExportable, IDependent
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
			m_curve = null;
		}

		public PPtrCurve(string path, string attribute, ClassIDType classID, PPtr<MonoScript> script, IReadOnlyList<PPtrKeyframe> keyframes) :
			this(path, attribute, classID, script)
		{
			m_curve = new PPtrKeyframe[keyframes.Count];
			for (int i = 0; i < keyframes.Count; i++)
			{
				m_curve[i] = keyframes[i];
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

		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(2017);
		}

		public void Read(AssetReader reader)
		{
			m_curve = reader.ReadAssetArray<PPtrKeyframe>();
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}

			Attribute = reader.ReadString();
			Path = reader.ReadString();
			ClassID = (ClassIDType)reader.ReadInt32();
			Script.Read(reader);
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

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in context.FetchDependencies(Curve, CurveName))
			{
				yield return asset;
			}
			yield return context.FetchDependency(Script, ScriptName);
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

		public IReadOnlyList<PPtrKeyframe> Curve => m_curve;
		public string Attribute { get; private set; }
		public string Path { get; private set; }
		public ClassIDType ClassID { get; private set; }

		public const string CurveName = "curve";
		public const string AttributeName = "attribute";
		public const string PathName = "path";
		public const string ClassIDName = "classID";
		public const string ScriptName = "script";

		public PPtr<MonoScript> Script;

		private PPtrKeyframe[] m_curve;
	}
}
