using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;

namespace AssetRipper.Core.Classes.AnimatorController.Editor.AnimatorControllerLayer
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
			State = state.SerializedFile.CreatePPtr(state);
			Motion = motion.SerializedFile.CreatePPtr(motion);
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

		public PPtr<AnimatorState> State = new();
		public PPtr<Motion> Motion = new();
	}
}
