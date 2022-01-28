using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.AnimatorOverrideController
{
	public sealed class AnimationClipOverride : IAssetReadable, IYAMLExportable, IDependent
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

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(OriginalClip, OriginalClipName);
			yield return context.FetchDependency(OverrideClip, OverrideClipName);
		}

		public const string OriginalClipName = "m_OriginalClip";
		public const string OverrideClipName = "m_OverrideClip";

		public PPtr<AnimationClip.AnimationClip> OriginalClip = new();
		public PPtr<AnimationClip.AnimationClip> OverrideClip = new();
	}
}
