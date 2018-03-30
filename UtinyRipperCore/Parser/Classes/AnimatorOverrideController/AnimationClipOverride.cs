using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AnimatorOverrideControllers
{
	public struct AnimationClipOverride : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			OriginalClip.Read(stream);
			OverrideClip.Read(stream);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_OriginalClip", OriginalClip.ExportYAML(exporter));
			node.Add("m_OverrideClip", OverrideClip.ExportYAML(exporter));
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			AnimationClip clip = OriginalClip.FindObject(file);
			if (clip == null)
			{
				if(isLog)
				{
					Logger.Log(LogType.Warning, LogCategory.Export, $"AnimationClipOverride's m_OriginalClip {OriginalClip.ToLogString(file)} wasn't found ");
				}
			}
			else
			{
				yield return clip;
			}

			clip = OverrideClip.FindObject(file);
			if (clip == null)
			{
				if (isLog)
				{
					Logger.Log(LogType.Warning, LogCategory.Export, $"AnimationClipOverride's m_OverrideClip {OverrideClip.ToLogString(file)} wasn't found ");
				}
			}
			else
			{
				yield return clip;
			}
		}

		public PPtr<AnimationClip> OriginalClip;
		public PPtr<AnimationClip> OverrideClip;
	}
}
