using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class AnimatorTransition : AnimatorTransitionBase
	{
		public AnimatorTransition(VirtualSerializedFile file, AnimatorController controller, SelectorTransitionConstant transition) :
			   base(file, ClassIDType.AnimatorTransition, controller, transition)
		{
			file.AddAsset(this);
		}

		public AnimatorTransition(VirtualSerializedFile file, AnimatorController controller, SelectorTransitionConstant transition,
			IReadOnlyList<AnimatorState> states) :
			this(file, controller, transition)
		{
			DstState = PPtr<AnimatorState>.CreateVirtualPointer(states[transition.Destination]);
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
