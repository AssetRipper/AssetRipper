using AssetRipper.Core.Classes.AnimatorController.Constants;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.AnimatorTransition
{
	public sealed class AnimatorTransition : AnimatorTransitionBase
	{
		public class Parameters : BaseParameters
		{
			public override string Name => string.Empty;
			public override bool IsExit => false;
			public override int DestinationState => Transition.Destination;
			public SelectorTransitionConstant Transition { get; set; }
			public UnityVersion Version { get; set; }
			public override IReadOnlyList<OffsetPtr<ConditionConstant>> ConditionConstants => Transition.ConditionConstantArray;
		}

		private AnimatorTransition(LayoutInfo layout, AssetInfo assetInfo, Parameters parameters) :
			   base(layout, assetInfo, parameters)
		{ }

		public static AnimatorTransition CreateVirtualInstance(VirtualSerializedFile virtualFile, Parameters parameters)
		{
			return virtualFile.CreateAsset((assetInfo) => new AnimatorTransition(virtualFile.Layout, assetInfo, parameters));
		}

		public static int ToSerializedVersion(UnityVersion version)
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
