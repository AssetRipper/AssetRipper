using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AnimatorOverrideControllers
{
	public struct AnimationClipOverride : IAssetReadable, IYAMLExportable, IDependent
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
			yield return OriginalClip.FetchDependency(file, isLog, () => nameof(AnimationClipOverride), "m_OriginalClip");
			yield return OverrideClip.FetchDependency(file, isLog, () => nameof(AnimationClipOverride), "m_OverrideClip");
		}

		public PPtr<AnimationClip> OriginalClip;
		public PPtr<AnimationClip> OverrideClip;
	}
}
