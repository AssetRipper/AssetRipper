using System;
using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper.Classes.AnimatorControllers;
using uTinyRipper.Classes.AnimatorTransitions;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Layout;
using uTinyRipper.Classes.Objects;

namespace uTinyRipper.Classes
{
	public abstract class AnimatorTransitionBase : NamedObject
	{
		public abstract class BaseParameters
		{
			public AnimatorState GetDestinationState()
			{
				return GetDestinationState(DestinationState);
			}

			private AnimatorState GetDestinationState(int destinationState)
			{
				if (destinationState == -1)
				{
					return null;
				}
				else if (destinationState >= 30000)
				{
					// Entry and Exit states
					int stateIndex = destinationState % 30000;
					if (stateIndex == 0 || stateIndex == 1)
					{
						// base layer node. Default value is valid
						return null;
					}
					else
					{
						SelectorStateConstant selectorState = StateMachine.SelectorStateConstantArray[stateIndex].Instance;
						// HACK: take default Entry destination. TODO: child StateMachines
						SelectorTransitionConstant selectorTransition = selectorState.TransitionConstantArray[selectorState.TransitionConstantArray.Length - 1].Instance;
						return GetDestinationState(selectorTransition.Destination);
					}
				}
				else
				{
					return States[destinationState];
				}
			}

			public abstract string Name { get; }
			public abstract bool IsExit { get; }
			public abstract int DestinationState { get; }
			public StateMachineConstant StateMachine { get; set; }
			public IReadOnlyList<AnimatorState> States { get; set; }
			public IReadOnlyDictionary<uint, string> TOS { get; set; }
			public abstract IReadOnlyList<OffsetPtr<ConditionConstant>> ConditionConstants { get; }
		}

		protected AnimatorTransitionBase(AssetLayout layout, AssetInfo assetInfo, BaseParameters parameters) :
			base(layout)
		{
			AssetInfo = assetInfo;
			ObjectHideFlags = HideFlags.HideInHierarchy;

			List<AnimatorCondition> conditionList = new List<AnimatorCondition>(parameters.ConditionConstants.Count);
			for (int i = 0; i < parameters.ConditionConstants.Count; i++)
			{
				ConditionConstant conditionConstant = parameters.ConditionConstants[i].Instance;
				if (conditionConstant.ConditionMode != AnimatorConditionMode.ExitTime)
				{
					AnimatorCondition condition = new AnimatorCondition(conditionConstant, parameters.TOS);
					conditionList.Add(condition);
				}
			}
			Conditions = conditionList.ToArray();

			AnimatorState state = parameters.GetDestinationState();
			DstStateMachine = default;
			DstState = state == null ? default : state.File.CreatePPtr(state);

			Name = parameters.Name;
			Solo = false;
			Mute = false;
			IsExit = parameters.IsExit;

		}

		public sealed override void Read(AssetReader reader)
		{
			throw new NotSupportedException();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(ConditionsName, Conditions.ExportYAML(container));
			node.Add(DstStateMachineName, DstStateMachine.ExportYAML(container));
			node.Add(DstStateName, DstState.ExportYAML(container));
			node.Add(SoloName, Solo);
			node.Add(MuteName, Mute);
			node.Add(IsExitName, IsExit);
			return node;
		}

		public override string ExportExtension => throw new NotSupportedException();

		public AnimatorCondition[] Conditions { get; set; }
		public bool Solo { get; set; }
		public bool Mute { get; set; }
		public bool IsExit { get; set; }

		public const string ConditionsName = "m_Conditions";
		public const string DstStateMachineName = "m_DstStateMachine";
		public const string DstStateName = "m_DstState";
		public const string SoloName = "m_Solo";
		public const string MuteName = "m_Mute";
		public const string IsExitName = "m_IsExit";

		public PPtr<AnimatorStateMachine> DstStateMachine;
		public PPtr<AnimatorState> DstState;
	}
}
