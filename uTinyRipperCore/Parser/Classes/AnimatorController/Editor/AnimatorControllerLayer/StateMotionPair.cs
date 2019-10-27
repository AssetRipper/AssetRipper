using System;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers
{
	public sealed class StateMotionPair : IYAMLExportable
	{
		public StateMotionPair(AnimatorState state, Motion motion)
		{
			if (state == null)
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
			node.Add(StateName, State.ExportYAML(container));
			node.Add(MotionName, Motion.ExportYAML(container));
			return node;
		}

		public const string StateName = "m_State";
		public const string MotionName = "m_Motion";

		public PPtr<AnimatorState> State;
		public PPtr<Motion> Motion;
	}
}
