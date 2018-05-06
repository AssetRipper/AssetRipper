using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers.Editor
{
	public abstract class AnimatorTransitionBase : NamedObject
	{
		public AnimatorTransitionBase(AssetInfo assetsInfo) :
			base(assetsInfo)
		{
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
