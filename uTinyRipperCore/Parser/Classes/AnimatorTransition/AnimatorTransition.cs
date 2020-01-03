using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.Classes.AnimatorControllers;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Layout;

namespace uTinyRipper.Classes
{
	public sealed class AnimatorTransition : AnimatorTransitionBase
	{
		public class Parameters : BaseParameters
		{
			public override string Name => string.Empty;
			public override bool IsExit => false;
			public override int DestinationState => Transition.Destination;
			public SelectorTransitionConstant Transition { get; set; }
			public Version Version { get; set; }
			public override IReadOnlyList<OffsetPtr<ConditionConstant>> ConditionConstants => Transition.ConditionConstantArray;
		}

		private AnimatorTransition(AssetLayout layout, AssetInfo assetInfo, Parameters parameters) :
			   base(layout, assetInfo, parameters)
		{
		}

		public static AnimatorTransition CreateVirtualInstance(VirtualSerializedFile virtualFile, Parameters parameters)
		{
			return virtualFile.CreateAsset((assetInfo) => new AnimatorTransition(virtualFile.Layout, assetInfo, parameters));
		}

		public static int ToSerializedVersion(Version version)
		{
			return 1;
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.ForceAddSerializedVersion(ToSerializedVersion(container.Version));
			return node;
		}
	}
}
