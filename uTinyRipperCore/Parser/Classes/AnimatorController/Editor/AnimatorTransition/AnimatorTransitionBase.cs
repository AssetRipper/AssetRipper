using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Objects;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers.Editor
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
						SelectorTransitionConstant selectorTransition = selectorState.TransitionConstantArray[selectorState.TransitionConstantArray.Count - 1].Instance;
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

		protected AnimatorTransitionBase(AssetInfo assetInfo, ClassIDType classID, BaseParameters parameters) :
			base(assetInfo, HideFlags.HideInHierarchy)
		{
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
			m_conditions = conditionList.ToArray();

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
			node.Add("m_Conditions", Conditions.ExportYAML(container));
			node.Add("m_DstStateMachine", DstStateMachine.ExportYAML(container));
			node.Add("m_DstState", DstState.ExportYAML(container));
			node.Add("m_Solo", Solo);
			node.Add("m_Mute", Mute);
			node.Add("m_IsExit", IsExit);
			return node;
		}

		public override string ExportExtension => throw new NotSupportedException();

		public IReadOnlyList<AnimatorCondition> Conditions => m_conditions;
		public bool Solo { get; private set; }
		public bool Mute { get; private set; }
		public bool IsExit { get; private set; }

		public PPtr<AnimatorStateMachine> DstStateMachine;
		public PPtr<AnimatorState> DstState;

		private AnimatorCondition[] m_conditions;
	}
}
