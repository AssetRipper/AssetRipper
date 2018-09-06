using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AnimationClips
{
	public struct PPtrKeyframe : IAssetReadable, IYAMLExportable, IDependent
	{
		public void Read(AssetReader reader)
		{
			Time = reader.ReadSingle();
			Script.Read(reader);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("time", Time);
			node.Add("value", Script.ExportYAML(container));
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Script.FetchDependency(file, isLog, () => nameof(PPtrKeyframe), "script");
		}

		public float Time { get; private set; }

		public PPtr<Object> Script;
	}
}
