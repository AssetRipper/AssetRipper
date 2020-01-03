using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes.AnimatorOverrideControllers
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
			node.Add(OriginalClipName, OriginalClip.ExportYAML(container));
			node.Add(OverrideClipName, OverrideClip.ExportYAML(container));
			return node;
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(OriginalClip, OriginalClipName);
			yield return context.FetchDependency(OverrideClip, OverrideClipName);
		}

		public const string OriginalClipName = "m_OriginalClip";
		public const string OverrideClipName = "m_OverrideClip";

		public PPtr<AnimationClip> OriginalClip;
		public PPtr<AnimationClip> OverrideClip;
	}
}
