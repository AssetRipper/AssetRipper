using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AnimationClips
{
	public struct FloatCurve : IAssetReadable, IYAMLExportable, IDependent
	{
		public FloatCurve(string path, IReadOnlyList<KeyframeTpl<Float>> keyframes)
		{
			Curve = new AnimationCurveTpl<Float>(keyframes);
			Attribute = string.Empty;
			Path = path;
			ClassID = 0;
			Script = default;
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
			Attribute = reader.ReadStringAligned();
			Path = reader.ReadStringAligned();
			ClassID = reader.ReadInt32();
			if (IsReadScript(reader.Version))
			{
				Script.Read(reader);
			}
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
			yield return Script.FetchDependency(file, isLog, () => nameof(FloatCurve), "script");
		}
		
		public string Attribute { get; private set; }
		public string Path { get; private set; }
		public int ClassID { get; private set; }

		public AnimationCurveTpl<Float> Curve;
		public PPtr<MonoScript> Script;
	}
}
