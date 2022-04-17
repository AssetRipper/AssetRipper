using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.AnimatorOverrideController
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

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(Controller, ControllerName);
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromArray(Clips, ClipsName))
			{
				yield return asset;
			}
		}

		public override bool IsContainsAnimationClip(AnimationClip.AnimationClip clip)
		{
			foreach (AnimationClipOverride overClip in Clips)
			{
				if (overClip.OriginalClip.IsAsset(SerializedFile, clip))
				{
					return true;
				}
				else if (overClip.OverrideClip.IsAsset(SerializedFile, clip))
				{
					return true;
				}
			}
			RuntimeAnimatorController baseController = Controller.FindAsset(SerializedFile);
			if (baseController != null)
			{
				return baseController.IsContainsAnimationClip(clip);
			}
			return false;
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.Add(ControllerName, Controller.ExportYaml(container));
			node.Add(ClipsName, Clips.ExportYaml(container));
			return node;
		}

		public override string ExportExtension => "overrideController";

		public AnimationClipOverride[] Clips { get; set; }

		public const string ControllerName = "m_Controller";
		public const string ClipsName = "m_Clips";

		public PPtr<RuntimeAnimatorController> Controller = new();
	}
}
