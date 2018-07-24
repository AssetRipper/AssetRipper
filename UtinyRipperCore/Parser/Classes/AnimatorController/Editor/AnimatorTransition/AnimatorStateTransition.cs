using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class AnimatorStateTransition : AnimatorTransitionBase
	{
		public AnimatorStateTransition(VirtualSerializedFile file, AnimatorController controller, TransitionConstant transition) :
			base(file, ClassIDType.AnimatorStateTransition, controller, transition)
		{
			TransitionDuration = transition.TransitionDuration;
			TransitionOffset = transition.TransitionOffset;
			ExitTime = transition.GetExitTime(controller.File.Version);
			HasExitTime = transition.GetHasExitTime(controller.File.Version);
			HasFixedDuration = transition.GetHasFixedDuration(controller.File.Version); ;
			InterruptionSource = transition.GetInterruptionSource(controller.File.Version);
			OrderedInterruption = transition.OrderedInterruption;
			CanTransitionToSelf = transition.CanTransitionToSelf;

			file.AddAsset(this);
		}

		public AnimatorStateTransition(VirtualSerializedFile file, AnimatorController controller, TransitionConstant transition,
			IReadOnlyList<AnimatorState> states) :
			this(file, controller, transition)
		{
			if(!transition.IsExit)
			{
				DstState = PPtr<AnimatorState>.CreateVirtualPointer(states[transition.DestinationState]);
			}
		}

		private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 3;
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_TransitionDuration", TransitionDuration);
			node.Add("m_TransitionOffset", TransitionOffset);
			node.Add("m_ExitTime", ExitTime);
			node.Add("m_HasExitTime", HasExitTime);
			node.Add("m_HasFixedDuration", HasFixedDuration);
			node.Add("m_InterruptionSource", (int)InterruptionSource);
			node.Add("m_OrderedInterruption", OrderedInterruption);
			node.Add("m_CanTransitionToSelf", CanTransitionToSelf);
			return node;
		}

		public float TransitionDuration { get; private set; }
		public float TransitionOffset { get; private set; }
		public float ExitTime { get; private set; }
		public bool HasExitTime { get; private set; }
		public bool HasFixedDuration { get; private set; }
		public TransitionInterruptionSource InterruptionSource { get; private set; }
		public bool OrderedInterruption { get; private set; }
		public bool CanTransitionToSelf { get; private set; }
	}
}
