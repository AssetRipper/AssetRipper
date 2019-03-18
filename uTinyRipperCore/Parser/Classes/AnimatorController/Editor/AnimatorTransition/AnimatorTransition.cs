using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class AnimatorTransition : AnimatorTransitionBase
	{
		private AnimatorTransition(AssetInfo assetInfo, AnimatorController controller, SelectorTransitionConstant transition) :
			   base(assetInfo, ClassIDType.AnimatorTransition, controller, transition)
		{
		}

		private AnimatorTransition(AssetInfo assetInfo, AnimatorController controller, SelectorTransitionConstant transition, IReadOnlyList<AnimatorState> states) :
			this(assetInfo, controller, transition)
		{
			AnimatorState state = states[transition.Destination];
			DstState = state.File.CreatePPtr(state);
		}

		public static AnimatorTransition CreateVirtualInstance(VirtualSerializedFile virtualFile, AnimatorController controller, SelectorTransitionConstant transition)
		{
			return virtualFile.CreateAsset((assetInfo) => new AnimatorTransition(assetInfo, controller,	transition));
		}

		public static AnimatorTransition CreateVirtualInstance(VirtualSerializedFile virtualFile, AnimatorController controller, SelectorTransitionConstant transition,
			IReadOnlyList<AnimatorState> states)
		{
			return virtualFile.CreateAsset((assetInfo) => new AnimatorTransition(assetInfo, controller, transition, states));
		}

		private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 1;
		}
		
		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.ForceAddSerializedVersion(GetSerializedVersion(container.Version));
			return node;
		}
	}
}
