using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AnimatorOverrideControllers
{
	public struct AnimationClipOverride : IAssetReadable, IYAMLExportable, IDependent
	{
		public void Read(AssetReader reader)
		{
			OriginalClip.Read(reader);
			OverrideClip.Read(reader);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_OriginalClip", OriginalClip.ExportYAML(container));
			node.Add("m_OverrideClip", OverrideClip.ExportYAML(container));
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
