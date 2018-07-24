using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AnimatorControllers.Editor
{
	public abstract class AnimatorTransitionBase : NamedObject
	{
		private AnimatorTransitionBase(VirtualSerializedFile file, ClassIDType classID, AnimatorController controller, IReadOnlyList<OffsetPtr<ConditionConstant>> conditions) :
			base(file.CreateAssetInfo(classID))
		{
			ObjectHideFlags = 1;

			List<AnimatorCondition> conditionList = new List<AnimatorCondition>(conditions.Count);
			for (int i = 0; i < conditions.Count; i++)
			{
				ConditionConstant conditionConstant = conditions[i].Instance;
				if(conditionConstant.ConditionMode != AnimatorConditionMode.ExitTime)
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

		protected AnimatorTransitionBase(VirtualSerializedFile file, ClassIDType classID, AnimatorController controller, TransitionConstant transition) :
			this(file, classID, controller, transition.ConditionConstantArray)
		{
			Name = controller.TOS[transition.UserID];
			IsExit = transition.IsExit;
		}

		protected AnimatorTransitionBase(VirtualSerializedFile file, ClassIDType classID, AnimatorController controller, SelectorTransitionConstant transition) :
			this(file, classID, controller, transition.ConditionConstantArray)
		{
			Name = string.Empty;
			IsExit = false;
		}

		public sealed override void Read(AssetStream stream)
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
