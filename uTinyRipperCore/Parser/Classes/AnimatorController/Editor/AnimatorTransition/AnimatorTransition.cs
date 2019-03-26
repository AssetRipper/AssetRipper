using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using System.Collections.Generic;

namespace uTinyRipper.Classes.AnimatorControllers.Editor
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

		private AnimatorTransition(AssetInfo assetInfo, Parameters parameters) :
			   base(assetInfo, ClassIDType.AnimatorTransition, parameters)
		{
		}

		public static AnimatorTransition CreateVirtualInstance(VirtualSerializedFile virtualFile, Parameters parameters)
		{
			return virtualFile.CreateAsset((assetInfo) => new AnimatorTransition(assetInfo, parameters));
		}

		private static int GetSerializedVersion(Version version)
		{
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
