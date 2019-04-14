using System;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers.Editor
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

		private static int GetSerializedVersion(Version version)
		{
			// TODO:
			return 1;
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.ForceAddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add("m_State", State.ExportYAML(container));
			node.Add("m_Position", Position.ExportYAML(container));
			return node;
		}

		public PPtr<AnimatorState> State;
		public Vector3f Position;
	}
}
