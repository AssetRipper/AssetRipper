using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.Classes.AnimatorControllers;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Layout;

namespace uTinyRipper.Classes
{
	public sealed class AnimatorStateTransition : AnimatorTransitionBase
	{
		public class Parameters : BaseParameters
		{
			public override string Name => TOS[Transition.UserID];
			public override bool IsExit => Transition.IsExit;
			public override int DestinationState => Transition.DestinationState;
			public TransitionConstant Transition { get; set; }
			public Version Version { get; set; }
			public override IReadOnlyList<OffsetPtr<ConditionConstant>> ConditionConstants => Transition.ConditionConstantArray;
		}

		private AnimatorStateTransition(AssetLayout layout, AssetInfo assetInfo, Parameters parameters) :
			base(layout, assetInfo, parameters)
		{
			TransitionDuration = parameters.Transition.TransitionDuration;
			TransitionOffset = parameters.Transition.TransitionOffset;
			ExitTime = parameters.Transition.GetExitTime(parameters.Version);
			HasExitTime = parameters.Transition.GetHasExitTime(parameters.Version);
			HasFixedDuration = parameters.Transition.GetHasFixedDuration(parameters.Version);
			InterruptionSource = parameters.Transition.GetInterruptionSource(parameters.Version);
			OrderedInterruption = parameters.Transition.OrderedInterruption;
			CanTransitionToSelf = parameters.Transition.CanTransitionToSelf;
		}

		public static AnimatorStateTransition CreateVirtualInstance(VirtualSerializedFile virtualFile, Parameters parameters)
		{
			return virtualFile.CreateAsset((assetInfo) => new AnimatorStateTransition(virtualFile.Layout, assetInfo, parameters));
		}

		public static int ToSerializedVersion(Version version)
		{
			// TODO:
			return 3;
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(TransitionDurationName, TransitionDuration);
			node.Add(TransitionOffsetName, TransitionOffset);
			node.Add(ExitTimeName, ExitTime);
			node.Add(HasExitTimeName, HasExitTime);
			node.Add(HasFixedDurationName, HasFixedDuration);
			node.Add(InterruptionSourceName, (int)InterruptionSource);
			node.Add(OrderedInterruptionName, OrderedInterruption);
			node.Add(CanTransitionToSelfName, CanTransitionToSelf);
			return node;
		}

		public float TransitionDuration { get; set; }
		public float TransitionOffset { get; set; }
		public float ExitTime { get; set; }
		public bool HasExitTime { get; set; }
		public bool HasFixedDuration { get; set; }
		public TransitionInterruptionSource InterruptionSource { get; set; }
		public bool OrderedInterruption { get; set; }
		public bool CanTransitionToSelf { get; set; }

		public const string TransitionDurationName = "m_TransitionDuration";
		public const string TransitionOffsetName = "m_TransitionOffset";
		public const string ExitTimeName = "m_ExitTime";
		public const string HasExitTimeName = "m_HasExitTime";
		public const string HasFixedDurationName = "m_HasFixedDuration";
		public const string InterruptionSourceName = "m_InterruptionSource";
		public const string OrderedInterruptionName = "m_OrderedInterruption";
		public const string CanTransitionToSelfName = "m_CanTransitionToSelf";
	}
}
