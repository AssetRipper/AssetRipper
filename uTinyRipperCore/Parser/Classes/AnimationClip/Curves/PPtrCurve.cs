using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct PPtrCurve : IAssetReadable, IYAMLExportable, IDependent
	{
		public PPtrCurve(PPtrCurve copy, IReadOnlyList<PPtrKeyframe> keyframes):
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

		public PPtrCurve(string path, string attribute, ClassIDType classID, PPtr<MonoScript> script, IReadOnlyList<PPtrKeyframe> keyframes):
			this(path, attribute, classID, script)
		{
			m_curve = new PPtrKeyframe[keyframes.Count];
			for(int i = 0; i < keyframes.Count; i++)
			{
				m_curve[i] = keyframes[i];
			}
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
				reader.AlignStream(AlignType.Align4);
			}

			Attribute = reader.ReadString();
			Path = reader.ReadString();
			ClassID = (ClassIDType)reader.ReadInt32();
			Script.Read(reader);
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

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(PPtrKeyframe keyframe in Curve)
			{
				foreach (Object asset in keyframe.FetchDependencies(file, isLog))
				{
					yield return asset;
				}
			}
			yield return Script.FetchDependency(file, isLog, () => nameof(PPtrCurve), "script");
		}

		public override bool Equals(object obj)
		{
			if(obj == null)
			{
				return false;
			}
			if(obj is PPtrCurve pptrCurve)
			{
				if(pptrCurve.Attribute != Attribute)
				{
					return false;
				}
				if (pptrCurve.Path != Path)
				{
					return false;
				}
				if (pptrCurve.ClassID != ClassID)
				{
					return false;
				}
				if (pptrCurve.Script != Script)
				{
					return false;
				}
				return true;
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

		public PPtr<MonoScript> Script;

		private PPtrKeyframe[] m_curve;
	}
}
