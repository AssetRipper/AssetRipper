using System;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorStateMachines
{
	public sealed class ChildAnimatorState : IYAMLExportable
	{
		public ChildAnimatorState(AnimatorState state, Vector3f position)
		{
			if (state == null)
			{
				throw new ArgumentNullException(nameof(state));
			}
			State = state.File.CreatePPtr(state);
			Position = position;
		}

		public static int ToSerializedVersion(Version version)
		{
			// TODO:
			return 1;
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.ForceAddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(StateName, State.ExportYAML(container));
			node.Add(PositionName, Position.ExportYAML(container));
			return node;
		}

		public const string StateName = "m_State";
		public const string PositionName = "m_Position";

		public PPtr<AnimatorState> State;
		public Vector3f Position;
	}
}
