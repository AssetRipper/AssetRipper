using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.AnimatorController.Editor.AnimatorControllerLayer
{
	public sealed class StateMotionPair : IYamlExportable
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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(StateName, State.ExportYaml(container));
			node.Add(MotionName, Motion.ExportYaml(container));
			return node;
		}

		public const string StateName = "m_State";
		public const string MotionName = "m_Motion";

		public PPtr<AnimatorState> State = new();
		public PPtr<Motion> Motion = new();
	}
}
