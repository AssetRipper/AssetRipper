using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.AnimationClips
{
	public struct PPtrCurve : IAssetReadable, IYAMLExportable, IDependent
	{
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(2017);
		}

		public void Read(AssetReader reader)
		{
			m_curve = reader.ReadArray<PPtrKeyframe>();
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			Attribute = reader.ReadStringAligned();
			Path = reader.ReadStringAligned();
			ClassID = reader.ReadInt32();
			Script.Read(reader);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("curve", Curve.ExportYAML(container));
			node.Add("attribute", Attribute);
			node.Add("path", Path);
			node.Add("classID", ClassID);
			node.Add("script", Script.ExportYAML(container));
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(PPtrKeyframe keyframe in Curve)
			{
				foreach (Object @object in keyframe.FetchDependencies(file, isLog))
				{
					yield return @object;
				}
			}
			yield return Script.FetchDependency(file, isLog, () => nameof(PPtrCurve), "script");
		}

		public IReadOnlyList<PPtrKeyframe> Curve => m_curve;
		public string Attribute { get; private set; }
		public string Path { get; private set; }
		public int ClassID { get; private set; }

		public PPtr<MonoScript> Script;

		private PPtrKeyframe[] m_curve;
	}
}
