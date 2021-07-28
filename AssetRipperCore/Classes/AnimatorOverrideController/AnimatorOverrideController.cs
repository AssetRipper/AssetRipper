using AssetRipper.Project;
using AssetRipper.Parser.Asset;
using AssetRipper.Classes.Misc;
using AssetRipper.IO.Asset;
using AssetRipper.IO.Extensions;
using AssetRipper.YAML;
using System.Collections.Generic;

namespace AssetRipper.Classes.AnimatorOverrideController
{
	public sealed class AnimatorOverrideController : RuntimeAnimatorController
	{
		public AnimatorOverrideController(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Controller.Read(reader);
			Clips = reader.ReadAssetArray<AnimationClipOverride>();
		}

		public override IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object.Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(Controller, ControllerName);
			foreach (PPtr<Object.Object> asset in context.FetchDependencies(Clips, ClipsName))
			{
				yield return asset;
			}
		}

		public override bool IsContainsAnimationClip(AnimationClip.AnimationClip clip)
		{
			foreach (AnimationClipOverride overClip in Clips)
			{
				if (overClip.OriginalClip.IsAsset(File, clip))
				{
					return true;
				}
				else if (overClip.OverrideClip.IsAsset(File, clip))
				{
					return true;
				}
			}
			RuntimeAnimatorController baseController = Controller.FindAsset(File);
			if (baseController != null)
			{
				return baseController.IsContainsAnimationClip(clip);
			}
			return false;
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(ControllerName, Controller.ExportYAML(container));
			node.Add(ClipsName, Clips.ExportYAML(container));
			return node;
		}

		public override string ExportExtension => "overrideController";

		public AnimationClipOverride[] Clips { get; set; }

		public const string ControllerName = "m_Controller";
		public const string ClipsName = "m_Clips";

		public PPtr<RuntimeAnimatorController> Controller;
	}
}
