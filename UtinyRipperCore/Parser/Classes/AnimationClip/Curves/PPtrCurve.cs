using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AnimationClips
{
	public struct PPtrCurve : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(2017);
		}

		public void Read(AssetStream stream)
		{
			m_curve = stream.ReadArray<PPtrKeyframe>();
			if (IsAlign(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}

			Attribute = stream.ReadStringAligned();
			Path = stream.ReadStringAligned();
			ClassID = stream.ReadInt32();
			Script.Read(stream);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("curve", Curve.ExportYAML(exporter));
			node.Add("attribute", Attribute);
			node.Add("path", Path);
			node.Add("classID", ClassID);
			node.Add("script", Script.ExportYAML(exporter));
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
			MonoScript script = Script.FindObject(file);
			if(script == null)
			{
				if(isLog)
				{
					Logger.Log(LogType.Warning, LogCategory.Export, $"PPtrCurve's script {Script.ToLogString(file)} wasn't found ");
				}
			}
			else
			{
				yield return script;
			}
		}

		public IReadOnlyList<PPtrKeyframe> Curve => m_curve;
		public string Attribute { get; private set; }
		public string Path { get; private set; }
		public int ClassID { get; private set; }

		public PPtr<MonoScript> Script;

		private PPtrKeyframe[] m_curve;
	}
}
