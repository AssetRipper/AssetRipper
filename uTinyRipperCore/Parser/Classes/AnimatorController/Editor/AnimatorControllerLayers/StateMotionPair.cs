using System;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class StateMotionPair : IYAMLExportable
	{
		public StateMotionPair(AnimatorState state, Motion motion)
		{
			if(state == null)
			{
				throw new ArgumentNullException(nameof(state));
			}
			if (motion == null)
			{
				throw new ArgumentNullException(nameof(motion));
			}
			State = state.File.CreatePPtr(state);
			Motion = motion.File.CreatePPtr(motion);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_State", State.ExportYAML(container));
			node.Add("m_Motion", Motion.ExportYAML(container));
			return node;
		}

		public PPtr<AnimatorState> State;
		public PPtr<Motion> Motion;
	}
}
