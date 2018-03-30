using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AnimationClips
{
	public struct PPtrKeyframe : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			Time = stream.ReadSingle();
			Script.Read(stream);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("time", Time);
			node.Add("value", Script.ExportYAML(exporter));
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			Object script = Script.FindObject(file);
			if(script == null)
			{
				if(isLog)
				{
					Logger.Log(LogType.Warning, LogCategory.Export, $"PPtrKeyframe's script {Script.ToLogString(file)} wasn't found ");
				}
			}
			else
			{
				yield return script;
			}
		}

		public float Time { get; private set; }

		public PPtr<Object> Script;
	}
}
