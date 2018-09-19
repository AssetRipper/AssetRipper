using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers.Editor
{
	public abstract class AnimatorTransitionBase : NamedObject
	{
		protected AnimatorTransitionBase(AssetInfo assetInfo, ClassIDType classID, AnimatorController controller, TransitionConstant transition) :
			this(assetInfo, classID, controller, transition.ConditionConstantArray)
		{
			Name = controller.TOS[transition.UserID];
			IsExit = transition.IsExit;
		}

		protected AnimatorTransitionBase(AssetInfo assetInfo, ClassIDType classID, AnimatorController controller, SelectorTransitionConstant transition) :
			this(assetInfo, classID, controller, transition.ConditionConstantArray)
		{
			IsExit = false;
		}

		private AnimatorTransitionBase(AssetInfo assetInfo, ClassIDType classID, AnimatorController controller, IReadOnlyList<OffsetPtr<ConditionConstant>> conditions) :
			base(assetInfo, 1)
		{
			List<AnimatorCondition> conditionList = new List<AnimatorCondition>(conditions.Count);
			for (int i = 0; i < conditions.Count; i++)
			{
				ConditionConstant conditionConstant = conditions[i].Instance;
				if (conditionConstant.ConditionMode != AnimatorConditionMode.ExitTime)
				{
					AnimatorCondition condition = new AnimatorCondition(controller, conditionConstant);
					conditionList.Add(condition);
				}
			}
			m_conditions = conditionList.ToArray();

			DstStateMachine = default;
			DstState = default;

			Solo = false;
			Mute = false;
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
